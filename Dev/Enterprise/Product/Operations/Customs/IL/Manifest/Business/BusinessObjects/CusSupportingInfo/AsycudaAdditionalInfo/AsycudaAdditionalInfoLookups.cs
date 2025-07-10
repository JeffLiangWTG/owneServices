using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaAdditionalInfoLookups : CusSupportingInfoLookups
	{
		public AsycudaAdditionalInfoLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public ICollection ReferenceNumberList
		{
			get
			{
				var statementType = Parent.CSI_Code;

				switch (statementType)
				{
					case Constants.AsycudaAdditionalInfoCodes.UNLOCO:
						return new RefUNLOCOCollection(Factory);
					case Constants.AsycudaAdditionalInfoCodes.ExporterTypeID:
						return new ILStatementCodeListForExporterIDType();
					case Constants.AsycudaAdditionalInfoCodes.NightStop:
					case Constants.AsycudaAdditionalInfoCodes.IsCooling:
					case Constants.AsycudaAdditionalInfoCodes.MultipleDeals:
						return new ILStatementCodeList();
					case Constants.AsycudaAdditionalInfoCodes.CargoType:
						return new ILStatementCodeListForCargoType();
					case Constants.AsycudaAdditionalInfoCodes.ActionCode:
						return new ILStatementCodeListForActionCode();
					default:
						return null;
				}
			}
		}

		public ICollection DescriptionList
		{
			get
			{
				var statementType = Parent.CSI_Code;

				switch (statementType)
				{
					case Constants.AsycudaAdditionalInfoCodes.IsDirectDelivery:
						return new ILContentListForIsDirectDelivery();
					default:
						return null;
				}
			}
		}

		public override ICollection CodeList
		{
			get
			{
				RefCusCodeListAttributeFilter[] attributeFilter = null;
				var parent = Parent.Parent;
				if (parent is AsycudaBill bill)
				{
					attributeFilter = new[]
						{
							new RefCusCodeListAttributeFilter(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Level, SQLComparisonOperator.Equal, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Bill)
						};
				}
				else if (parent is AsycudaPackedItem item)
				{
					attributeFilter = new[]
						{
							new RefCusCodeListAttributeFilter(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Level, SQLComparisonOperator.Equal, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Item)
						};
				}

				var result = new CodeDescriptionPairList();
				var list = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, ZDateTime.Today, attributeFilter);

				var code12 = list.FirstOrDefault(x => x.ZZD_Code == Constants.AsycudaAdditionalInfoCodes.ActionCode);
				if (code12 != null)
				{
					code12.ZZD_Description = Res.GetString("feb31d9c-a64d-4e62-8485-4946cc6421d6", "Action Code");
				}

				result.AddRange(list);
				result.Sort();
				return result;
			}
		}
	}
}
