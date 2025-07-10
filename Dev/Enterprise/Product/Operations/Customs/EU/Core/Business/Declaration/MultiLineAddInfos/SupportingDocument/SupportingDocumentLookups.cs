using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class SupportingDocumentLookups : CusSupportingInfoLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public override ICollection CodeList
		{
			get
			{
				var importExportParent = Parent.ImportExportParent as BusinessObject;
				if (importExportParent == null || importExportParent.IsDeleted)
				{
					return new CodeDescriptionPairList();
				}

				return Factory.GetSupportingDocumentList(Parent.ImportExportParent.DataGroupingCode, Parent.ParentDirection, Parent.ImportExportParent.Level, GetAdditionalCodeListAttributeFilters());
			}
		}

		public IEnumerable<ZString> PermitCodes
		{
			get
			{
				return Factory.GetCachedValue("EUPermitCodeChecker.PermitCodes" + Parent.ParentDirection, () =>
				{
					var importExportParent = Parent.ImportExportParent as BusinessObject;
					if (importExportParent == null || importExportParent.IsDeleted)
					{
						return new List<ZString>();
					}

					var codeList = Factory.GetSupportingDocumentListWithPermitAttribute(Parent.ImportExportParent.DataGroupingCode, Parent.ParentDirection);
					codeList.Load();
					return codeList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code);
				});
			}
		}

		protected virtual IEnumerable<RefCusCodeListAttributeFilter> GetAdditionalCodeListAttributeFilters() => null;

		public override CodeDescriptionPairList UnitOfQuantityList => !UnitOfQuantityListDataGroupingCode.IsEmpty ? RefCusCodeListTypes.GetCachedList(Factory, UnitOfQuantityListDataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsUnitOfMeasure, ZDateTime.Today) : new CodeDescriptionPairList();

		protected virtual ZString UnitOfQuantityListDataGroupingCode => Parent.ImportExportParent is ICanBeImportOrExport importExportParent ? importExportParent.DataGroupingCode : ZString.Empty;

		public CusPermitHeaderCollection Permits
		{
			get
			{
				var permitType = Parent.FormattedType;
				return Factory.GetCachedValue("EUSupportingDocumentLookups.Permits." + permitType, () =>
				{
					ZString countryCode = (Parent.Parent as ICanBeImportOrExport)?.TrueCountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					var permits = new CusPermitHeaderCollection(Factory, countryCode, CusPermitHeaderApplicationCodeList.Codes.Permit);
					permits.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusPermitHeaderCollection.FilterConstants.Country, "Property", countryCode, false));
					permits.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusPermitHeaderCollection.FilterConstants.PermitTypeSubType, "Property0", countryCode));
					permits.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(CusPermitHeaderCollection.FilterConstants.PermitTypeSubType, "Property3", permitType));
					return permits;
				});
			}
		}

		public CodeDescriptionPairList StatementTextList
		{
			get
			{
				var parent = Parent;
				var dataGroupingCode = parent.ImportExportParent?.DataGroupingCode;
				var direction = parent.ParentDirection;
				var documentCode = parent.CSI_Code;
				return Factory.GetCachedValue($"EUSupportingDocumentLookups.StatementText.{dataGroupingCode}.{direction}.{documentCode}", () =>
				{
					var importExportParent = parent.ImportExportParent as BusinessObject;
					if (importExportParent == null || importExportParent.IsDeleted)
					{
						return new CodeDescriptionPairList();
					}

					var result = new CodeDescriptionPairList();
					var refCusCode = Factory.GetSupportingDocumentCode(dataGroupingCode, direction, documentCode);
					if (refCusCode != null)
					{
						foreach (var statementText in refCusCode.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.StatementText))
						{
							result.AddPair(statementText);
						}
					}
					return result;
				});
			}
		}
	}
}
