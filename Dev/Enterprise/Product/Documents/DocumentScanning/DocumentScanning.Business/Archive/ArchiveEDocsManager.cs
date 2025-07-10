using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.DocumentScanning.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class ArchiveEDocsManager : AutoArchiveEDocs
	{
		public ArchiveEDocsManager(DocumentFactory factory)
			: base(factory)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SA_IncludeConsignee = true;
			SA_IncludeRelatedEDocs = true;
		}

		#region MasterFactory

		public DocumentFactory MasterFactory
		{
			get { return (DocumentFactory)Factory; }
		}

		#endregion

		#region Properties

		#region TotalSize

		/// <summary>
		/// Total Size of all documents being archived, in Megs
		/// </summary>
		public ZString TotalSize
		{
			get
			{
				if (!totalSize.HasValue)
				{
					long size = 0;
					foreach (StorageMain header in ListToArchive)
					{
						StorageDocsCollectionViewBase publishedDocs = header.PublishedEDocsAndFiles;
						foreach (StorageDocsBase element in publishedDocs)
						{
							size += GetSizeOfDocInDifferentFactory(element);
						}
					}

					var sizeInMegs = (int)Math.Ceiling(size / (double)(1024 * 1024));
					totalSize = sizeInMegs;
				}

				return totalSize.ToString();
			}
		}

		int? totalSize;

		public ZPropertyInfo TotalSizeInfo
		{
			get { return GetZPropertyInfo(nameof(TotalSize)); }
		}

		int GetSizeOfDocInDifferentFactory(StorageDocsBase element)
		{
			int result = 0;

			StorageDocsBase storageDocsBase = element.GetDocumentInNewFactory();

			if (storageDocsBase != null)
			{
				result = storageDocsBase.SC_ImageData.Length;
			}

			return result;
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region SelectedOrganisation

		public OrgHeader SelectedOrganisation
		{
			get { return (OrgHeader)MasterFactory.Load(typeof(OrgHeader), SA_Organisation); }
		}

		#endregion

		#endregion

		#region Bind To Lists

		#region OrganisationCollection

		public OrgHeaderCollection OrganisationList
		{
			get
			{
				if (fOrganisationList == null)
				{
					fOrganisationList = new OrgHeaderCollection(MasterFactory);
				}
				return fOrganisationList;
			}
		}

		OrgHeaderCollection fOrganisationList;

		#endregion

		#region ListToArchive

		public StorageMainCollection ListToArchive
		{
			get
			{
				if (fListToArchive == null)
				{
					fListToArchive = new StorageMainCollection(MasterFactory);
					fListToArchive.CountChanged += new CollectionCountChangedEventHandler(UpdateTotalSize);
				}
				return fListToArchive;
			}
		}

		StorageMainCollection fListToArchive;

		#endregion

		#endregion

		public SearchTypeCollection SearchTypes
		{
			get
			{
				if (searchTypes == null)
				{
					searchTypes = new SearchTypeCollection();
					foreach (KeyValuePair<string, IAssemblyData> pair in AssemblyDataLookup.AllAssemblyData)
					{
						if (pair.Value.GetQuery(new AssemblyDataParams()) != null)
						{
							searchTypes.Add(new SearchType(pair.Value, this));
						}
					}
				}
				return searchTypes;
			}
		}
		SearchTypeCollection searchTypes;

		public ZBool IsSearchTypeSelected
		{
			get { return SearchTypes.IsAtLeastOneSelected; }
		}

		public ZPropertyInfo IsSearchTypeSelectedInfo
		{
			get { return GetZPropertyInfo(nameof(IsSearchTypeSelected)); }
		}

		public void CreateArchiveList()
		{
			if (SA_IncludeConsignee || SA_IncludeConsignor)
			{
				int previousCount = ListToArchive.Count;
				AssemblyDataParams assemblyDataParams = new AssemblyDataParams(SA_IncludeConsignee, SA_IncludeConsignor, SA_ETDFrom, SA_ETDTo, SA_ETAFrom, SA_ETATo, SA_JobClosedFrom, SA_JobClosedTo, SA_Organisation);

				foreach (SearchType searchType in SearchTypes)
				{
					ZQuery query = searchType.AssemblyData.GetQuery(assemblyDataParams);
					if (query != null && searchType.IsFilterOn)
					{
						AddToArchiveListFromQuery(searchType.AssemblyData.BusinessObjectType, query);
					}
				}

				if (ListToArchive.Count == previousCount)
				{
					if (NoDocumentsAddedToList != null)
					{
						NoDocumentsAddedToList(this, EventArgs.Empty);
					}
				}
				else
				{
					ResetTotalSize();
				}
			}
			else
			{
				if (NoConsigneeOrConsignorSelected != null)
				{
					NoConsigneeOrConsignorSelected(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler NoDocumentsAddedToList;
		public event EventHandler NoConsigneeOrConsignorSelected;

		void AddToArchiveListFromQuery(Type typeToLoad, ZQuery query)
		{
			BusinessObject[] matchingBizOs = MasterFactory.Load(typeToLoad, query);

			foreach (BusinessObject bizO in matchingBizOs)
			{
				AddToArchiveListFromBizO(bizO);

				if (SA_IncludeRelatedEDocs)
				{
					IDocManagerSupport docManagerSupport = bizO as IDocManagerSupport;
					if (docManagerSupport != null)
					{
						DocManagerInfo docManagerInfo = docManagerSupport.DocManagerInfo;
						if (docManagerInfo != null)
						{
							BusinessObject[] relatedBizOs = docManagerInfo.RelatedObjects;
							if (relatedBizOs != null)
							{
								foreach (BusinessObject relatedBizO in relatedBizOs)
								{
									AddToArchiveListFromBizO(relatedBizO);
								}
							}
						}
					}
				}
			}
		}

		void AddToArchiveListFromBizO(BusinessObject bizO)
		{
			StorageMain parent = MasterFactory.GetStorageMainForPK(bizO.PK);
			if (parent != null)
			{
				if (MasterFactory.GetCountOfPublishedDocumentsAndFilesForStorageMain(parent) > 0) // don't load docs into the factory, just query the db
				{
					ListToArchive.Add(parent);
				}
			}
		}

		void UpdateTotalSize(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is StorageMain)
			{
				StorageMain parentMain = (StorageMain)e.BizObject;
				if (e.ItemAdded)
				{
					parentMain.PublishedEDocsAndFiles.CountChanged += new CollectionCountChangedEventHandler(UpdateTotalSize);
				}
				else if (e.ItemRemoved)
				{
					parentMain.PublishedEDocsAndFiles.CountChanged -= new CollectionCountChangedEventHandler(UpdateTotalSize);
				}
			}

			if (e.ItemRemoved)
			{
				ResetTotalSize();
			}
		}

		void ResetTotalSize()
		{
			totalSize = null;
			TotalSizeInfo.RefreshBinding();
			GC.Collect(GC.GetGeneration(Factory)); // Reclaiming memory after a document has been removed from the collection
		}
	}
}
