using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business
{
	public sealed class DIFDocumentLookups : ZLookups
	{
		public DIFDocumentLookups(AutoDIFDocument bizObj)
			: base(bizObj)
		{
		}

		new DIFDocument Parent
		{
			get { return (DIFDocument)base.Parent; }
		}

		IDISHost DISHost => Parent.HostWrapper?.DISHost;

		public IBusinessObjectCollection RequiredDocuments
		{
			get { return DISHost?.RequiredDocumentsProvider?.RequiredDocuments; }
		}

		public CodeDescriptionPairList EDocsList
		{
			get
			{
				return Parent.Factory.GetCachedValue(GetCachedKey("DIF_EDocsList"), delegate
				{
					var result = new CodeDescriptionPairList();
					foreach (var eDoc in DISHost?.EDocs)
					{
						if (!eDoc.IsDeleted)
						{
							result.AddPair(eDoc.UniqueKey, eDoc.DocType.PadRight(3) + "-" + eDoc.FileName, "Added: " + eDoc.DateAdded.ToShortDateString() + " - " + eDoc.Description);
						}
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList StatusList => Factory.GetCachedValue<StatusList>();

		public CodeDescriptionPairList BusinessNumbers
		{
			get
			{
				var result = new CodeDescriptionPairList();

				foreach (var currentComNum in CurrentCompanyNumbers)
				{
					result.AddPair(currentComNum.OK_CustomsRegNo.ToString(), currentComNum.OK_CodeType + " (" + CurrentCompanyText + ")");
				}

				if (ImporterNumbers != null)
				{
					foreach (var importerNum in ImporterNumbers)
					{
						result.AddPair(importerNum.OK_CustomsRegNo.ToString(), importerNum.OK_CodeType + " (" + ImporterCompanyText + " - " + ImporterCode + ")");
					}
				}

				return result;
			}
		}

		internal IEnumerable<OrgCusCode> CurrentCompanyNumbers =>
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.Cast<OrgCusCode>()
				.Where(code => BusinessNumberCodeTypesForDIF.GetDIFBusinessNumberCodeTypes().Contains(code.OK_CodeType));

		IEnumerable<OrgCusCode> importerNumbers;

		internal IEnumerable<OrgCusCode> ImporterNumbers
		{
			get
			{
				if (importerNumbers == null)
				{
					importerNumbers = Parent.HostWrapper?.DISHost?.BusinessNumberHolder?.CustomsCodes?.Cast<OrgCusCode>()
						.Where(code => BusinessNumberCodeTypesForDIF.GetDIFBusinessNumberCodeTypes().Contains(code.OK_CodeType));
				}
				return importerNumbers;
			}
		}

		ZString ImporterCode => Parent.HostWrapper?.DISHost?.BusinessNumberHolder?.OH_Code ?? ZString.Empty;

		static MultilingualString CurrentCompanyText => ResString.GetMultilingualString("86F7099A-CFDE-4FF9-B5DF-F8505C9168EC", "Broker");

		static MultilingualString ImporterCompanyText => ResString.GetMultilingualString("BAAF458B-BF5D-45A8-87DF-E5699468B84C", "Importer");

		public CodeDescriptionPairList PGAs => Factory.GetCachedValue<PGACodes>();

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList DocumentTypes
		{
			get
			{
				var pgaCode = Parent.PGA;
				var date = ZDateTime.UtcToday.Date;
				return Factory.GetCachedValue(string.Format("CADocumentType_PGA_{0}_{1}", pgaCode, date.ToShortDateString()), () =>
				{
					ZZRefCusCodeListCombinedCollection result;
					if (pgaCode == PGACodes.Codes.CFIA)
					{
						result = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType, date);
					}
					else
					{
						result = new ZZRefCusCodeListCombinedCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Canada }, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType }, date, new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, SQLComparisonOperator.Equal, pgaCode) });
					}
					result.Load();
					return result;
				});
			}
		}

		string GetCachedKey(string listName)
		{
			return Parent.PK.ToStringKey() + listName;
		}
	}
}
