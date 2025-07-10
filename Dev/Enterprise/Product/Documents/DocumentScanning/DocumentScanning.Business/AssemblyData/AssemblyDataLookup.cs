using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.DocumentScanning.Business
{
	public sealed partial class AssemblyDataLookup : Integration.IAssemblyDataLookup
	{
		[ThreadStatic]
		static ThreadLocalState threadLocalState;

		public static DocManagerCodesCodeDescriptionPairList AllAssemblyDataRegardlessOfCompany
		{
			get
			{
				var assemblyDataRegardlessOfCompany = State.AllAssemblyDataRegardlessOfCompany;
				if (assemblyDataRegardlessOfCompany == null)
				{
					SetStateSettingNull();
					var loader = new AssemblyDataLoader();
					State.AllAssemblyDataRegardlessOfCompany = loader.LoadRegardlessOfCompany();
				}
				return State.AllAssemblyDataRegardlessOfCompany;
			}
		}

		public static DocManagerCodesCodeDescriptionPairList AllAssemblyData
		{
			get
			{
				var currentCompany = GlbCompany.CurrentCompany;
				var assemblyData = State.AllAssemblyData;
				if (assemblyData == null
					|| (currentCompany != null && (assemblyData.Company.PK != currentCompany.PK
						|| assemblyData.CountryCode != currentCompany.GC_RN_NKCountryCode)))
				{
					SetStateSettingNull();
					var loader = new AssemblyDataLoader(currentCompany);
					State.AllAssemblyData = loader.Load();
				}
				return State.AllAssemblyData;
			}
		}

		static void SetStateSettingNull()
		{
			State.DocManagerCodesForAllocation = null;
			State.DocManagerCodes = null;
			State.ReferenceTypeList = null;
		}

		#region Lookups

		/// UPDATE LOCATION
		/// Lookup classes for UnallocatedDocuments grid and Plugin Tab. 
		/// 
		/// To enable the DocumentScanning plugin to support your BusinessObject, 
		/// copy the first AssemblyData item and override the following details:
		///
		/// 1. "ReferenceType" 3-letter code. Must be unique.
		/// 2. "HumanReadableName" e.g. "Declaration"
		/// 3. "BusinessObjectType" e.g. typeof(Enterprise.Accounting.Business.ARAP.Invoicing.ARInvoice).
		///		The business object must have a Code property marked with [CodeProperty(BizO.Schema.XXXX)] at the top of the class. 
		///		The BusinessObject must implement Enterprise.MasterFiles.Business.IDocManagerSupport
		/// 4. "DocTypeCategory" a 3 letter code describing the table name. Usually the same as your reference type 3 letter code.
		/// 
		/// Optionally, you can implement all 3 of these properties so users can allocate documents to your business object.
		/// 5. "ModuleID" specify the correct module ID for your business object so that the correct findbox is provided for 
		///		users to allocate documents to your business object.
		/// 6. "BusinessObjectCollectionType" used for the findbox. It must be non dependent with a constructor to accept only one parameter - BusinessObjectFactory.
		/// 7. "IsAllowedForUnallocatedeDocs" return true if unallocated documents can be assigned to this type of business object. Default is false.

		#endregion

		public static CodeDescriptionPairList ReferenceTypeList
		{
			get
			{
				if (State.ReferenceTypeList == null)
				{
					State.ReferenceTypeList = new CodeDescriptionPairList(OLookUpEditType.ReferenceTypes);
					State.ReferenceTypeList.Sort();
				}
				return State.ReferenceTypeList;
			}
		}

		/// <summary>
		/// This lookup is for use for UNALLOCATED documents. Some types are not allowed.
		/// </summary>
		public static CodeDescriptionPairList DocManagerCodesForAllocation
		{
			get
			{
				if (State.DocManagerCodesForAllocation == null)
				{
					// Local copy must be taken as calls to AllAssemblyData may nullify docManagerCodesForAllocation
					DocManagerCodesCodeDescriptionPairList allAssemblyData = AllAssemblyData;
					State.DocManagerCodesForAllocation = new CodeDescriptionPairList();

					foreach (var pair in allAssemblyData)
					{
						if (pair.Value.IsAllowedForUnallocatedeDocs)
						{
							State.DocManagerCodesForAllocation.AddPair(pair.Key, pair.Value.HumanReadableName);
						}
					}

					State.DocManagerCodesForAllocation.Sort();
				}

				return State.DocManagerCodesForAllocation;
			}
		}

		/// <summary>
		/// This separate lookup list is maintained for use in the RefDoctype form in MasterFiles.
		/// </summary>
		public static CodeDescriptionPairList DocManagerCodes
		{
			get
			{
				if (State.DocManagerCodes == null)
				{
					// Local copy must be taken as calls to AllAssemblyData may nullify docManagerCodesForAllocation
					var allAssemblyData = AllAssemblyData;
					State.DocManagerCodes = new CodeDescriptionPairList();
					foreach (var pair in allAssemblyData)
					{
						State.DocManagerCodes.AddPair(pair.Key, pair.Value.HumanReadableName);
					}

					State.DocManagerCodes.Sort();
				}

				return State.DocManagerCodes;
			}
		}

		public static bool IsDocManagerCodeValid(string docManagerCode)
		{
			return IsDocManagerCodeValid(docManagerCode, true);
		}

		public static bool IsDocManagerCodeValid(string docManagerCode, bool allowReload)
		{
			return GetAssemblyDataFromDocManagerCode(docManagerCode, allowReload) != null;
		}

		public static IAssemblyData GetAssemblyDataFromDocManagerCode(string docManagerCode)
		{
			return GetAssemblyDataFromDocManagerCode(docManagerCode, true);
		}

		public static IAssemblyData GetAssemblyDataFromDocManagerCode(string docManagerCode, bool allowReload)
		{
			var shouldIgnoreCompany = GlbCompany.CurrentCompany == null;
			var assemblyData = shouldIgnoreCompany
					? AllAssemblyDataRegardlessOfCompany.GetAssemblyDataFromDocManagerCode(docManagerCode)
					: AllAssemblyData.GetAssemblyDataFromDocManagerCode(docManagerCode);

			if (assemblyData != null || State.WasReloaded || !allowReload)
			{
				return assemblyData;
			}

			if (shouldIgnoreCompany)
			{
				State.AllAssemblyDataRegardlessOfCompany = null;
			}
			else
			{
				State.AllAssemblyData = null;
			}

			assemblyData = shouldIgnoreCompany
				? AllAssemblyDataRegardlessOfCompany.GetAssemblyDataFromDocManagerCode(docManagerCode)
				: AllAssemblyData.GetAssemblyDataFromDocManagerCode(docManagerCode);
			State.WasReloaded = true;

			return assemblyData;
		}

		public static string GetReferenceTypeFromDocManagerCode(string docManagerCode)
		{
			string result = AllAssemblyData.GetReferenceTypeFromDocManagerCode(docManagerCode);
			if (string.IsNullOrEmpty(result) && !State.WasReloaded)
			{
				State.AllAssemblyData = null;
				result = AllAssemblyData.GetReferenceTypeFromDocManagerCode(docManagerCode);
				State.WasReloaded = true;
			}
			return result;
		}

		public static IEnumerable<string> GetDocManagerCodes()
		{
			return AllAssemblyData.GetDocManagerCodes();
		}

		public static IAssemblyData Unallocated
		{
			get { return AllAssemblyData.Unallocated; }
		}

		#region GetPKFromCode

		public static ZGuid GetPKFromCode(DocumentFactory masterFactory, ZString docManagerCode, ZString code, bool throwExceptionIfDuplicated)
		{
			var bizo = GetBusinessObjectFromCode(masterFactory, docManagerCode, code, throwExceptionIfDuplicated);

			return bizo?.PK ?? ZGuid.Empty;
		}

		public static ZGuid GetPKFromCode(DocumentFactory masterFactory, ZString docManagerCode, ZString code, ZString companyCode, bool throwExceptionIfDuplicated)
		{
			return GetPKFromCode(docManagerCode, code, data => data.GetBusinessObjectCollection(masterFactory, new AssemblyDataParams { CompanyCode = companyCode }), throwExceptionIfDuplicated);
		}

		static ZGuid GetPKFromCode(ZString docManagerCode, ZString code, GetCollectionDelegate getCollection, bool throwExceptionIfDuplicated)
		{
			var bizo = GetBusinessObjectFromCode(docManagerCode, code, getCollection, throwExceptionIfDuplicated);

			return bizo?.PK ?? ZGuid.Empty;
		}

		delegate IBusinessObjectCollection GetCollectionDelegate(IAssemblyData data);

		#endregion

		#region GetBusinessObjectFromCode

		public static BusinessObject GetBusinessObjectFromCode(DocumentFactory masterFactory, ZString docManagerCode, ZString code, bool throwExceptionIfDuplicated)
		{
			var lastSpaceIndex = code.LastIndexOf(' ');
			if (lastSpaceIndex >= 0)
			{
				var bizo = GetBusinessObjectFromCode(masterFactory, docManagerCode, code.SubstringSafe(0, lastSpaceIndex), code.SubstringSafe(lastSpaceIndex + 1), throwExceptionIfDuplicated);
				if (bizo != null)
				{
					return bizo;
				}
			}

			return GetBusinessObjectFromCode(docManagerCode, code, data => data.GetBusinessObjectCollection(masterFactory), throwExceptionIfDuplicated);
		}

		public static BusinessObject GetBusinessObjectFromCode(DocumentFactory masterFactory, ZString docManagerCode, ZString code, ZString companyCode, bool throwExceptionIfDuplicated)
		{
			return GetBusinessObjectFromCode(docManagerCode, code, data => data.GetBusinessObjectCollection(masterFactory, new AssemblyDataParams { CompanyCode = companyCode }), throwExceptionIfDuplicated);
		}

		static BusinessObject GetBusinessObjectFromCode(ZString docManagerCode, ZString code, GetCollectionDelegate getCollection, bool throwExceptionIfDuplicated)
		{
			if (IsDocManagerCodeValid(docManagerCode))
			{
				IAssemblyData data = AllAssemblyData.GetAssemblyDataFromDocManagerCode(docManagerCode);
				IBusinessObjectCollection collection = getCollection(data);

				if (data.AllowLookupOfBizOFromPk)
				{
					if (ZGuid.TryParse(code, out var guid))
					{
						if (guid.IsValid && !guid.IsEmpty)
						{
							var bizO = collection.Factory.Load(collection.TypeOfElements, guid);
							if (bizO != null)
							{
								return bizO;
							}
						}
					}
				}

				if (collection != null)
				{
					var matchingBizOs = ((IFindBoxListProvider)collection).GetBusinessObjectsFromCode(code);
					if (matchingBizOs != null)
					{
						if (matchingBizOs.Count() > 1 && throwExceptionIfDuplicated)
						{
							throw new NonUniqueAllocationCodeException(Res.GetString("0a6006b5-547e-44d4-9df6-a0b5db7039d1", "The unique ID {0} you have supplied for Reference Type {1} exists in multiple records.", code, docManagerCode));
						}

						return matchingBizOs.FirstOrDefault();
					}
				}
			}

			return null;
		}

		#endregion

		static ThreadLocalState State
		{
			get
			{
				if (threadLocalState == null)
				{
					HookOnCreateNewThreadLocalState();
				}

				return threadLocalState = threadLocalState ?? new ThreadLocalState();
			}
		}

		static partial void HookOnCreateNewThreadLocalState();

		sealed class ThreadLocalState
		{
			public DocManagerCodesCodeDescriptionPairList AllAssemblyData { get; set; }
			public DocManagerCodesCodeDescriptionPairList AllAssemblyDataRegardlessOfCompany { get; set; }
			public CodeDescriptionPairList DocManagerCodesForAllocation { get; set; }
			public CodeDescriptionPairList DocManagerCodes { get; set; }
			public CodeDescriptionPairList ReferenceTypeList { get; set; }
			public bool WasReloaded { get; set; }
		}
	}
}

#region Test
#if DEBUG
namespace Enterprise.DocumentScanning.Business
{
	public sealed partial class AssemblyDataLookup
	{
		static readonly Overridable<IDisposable> testCleanUp = new Overridable<IDisposable>();

		public static void ClearDataForTesting()
		{
			threadLocalState = null;
		}

		internal static void ResetWasReloaded()
		{
			State.WasReloaded = false;
		}

#if DEBUG
		public
#else
		internal
#endif
		static bool StateIsSet()
		{
			return threadLocalState != null;
		}

		static partial void HookOnCreateNewThreadLocalState()
		{
			testCleanUp.Value = new DisposableAction(() => ClearDataForTesting());
		}

		public static void AddAssemblyDataForTesting(string code, IAssemblyData assemblyDataHolder)
		{
			AllAssemblyData.Add(code, assemblyDataHolder);
		}
	}
}
#endif
#endregion
