using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class EDocsDeleter
	{
		public EDocsDeleter(ICcsukCusAwb awb)
		{
			this.awb = awb;
		}

		internal void DeleteOldEdocs()
		{
			var listOfDocManagerInfos = new List<DocManagerInfo>();
			listOfDocManagerInfos.Add(awb.DocManagerInfo);
			foreach (IDocManagerSupport docManagerSupport in awb.DocManagerInfo.RelatedObjects)
			{
				listOfDocManagerInfos.Add(docManagerSupport.DocManagerInfo);
			}
			var eDocs = new List<EDocAndDocmanager>();
			foreach (var docManagerInfo in listOfDocManagerInfos)
			{
				foreach (IeDoc doc in docManagerInfo.AllEDocs)
				{
					eDocs.Add(new EDocAndDocmanager(doc, docManagerInfo));
				}
			}
			var uniqueList = (from EDocAndDocmanager ed in eDocs group ed by ed.EDoc.UniqueKey into g select g);
			foreach (var pair in uniqueList)
			{
				foreach (EDocAndDocmanager eDocAndManager in pair)
				{
					var referenceNumber = awb.ReferenceNumber;
					if (eDocAndManager.EDoc.Description.Contains(referenceNumber + " "))
					{
						if ((eDocAndManager.EDoc.DocType == "PUB" && eDocAndManager.EDoc.Description.Contains(" C1 ")) || eDocAndManager.EDoc.DocType == "RRA")
						{
							var sdb = eDocAndManager.EDoc;
							if (!sdb.IsDeleted)
							{
								sdb.IsDeleted = true;  // Move to the trash can
								WaterMarkOnExistingPfdAdder.UpdateStorageDocPdfToAddRedWatermark(sdb);  // Brand it with red text in case someone restores it
								awb.QueueForSaving(eDocAndManager.DocManagerInfo);  // Will be saved later on Awb.OnFactorySavingBeforeTransactionCore()
							}
						}
					}
				}
			}
		}

		readonly ICcsukCusAwb awb;

		class EDocAndDocmanager
		{
			public EDocAndDocmanager(IeDoc e, DocManagerInfo d)
			{
				EDoc = e;
				DocManagerInfo = d;
			}
			public IeDoc EDoc;
			public DocManagerInfo DocManagerInfo;
		}
	}
}
