using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using GlbStaffWrapper = Enterprise.Customs.JP.Common.GlbStaffWrapper;

namespace Enterprise.Customs.JP.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public OrgHeaderCollection RepresentativeList => new OrgHeaderCollection(Factory);

		new JobDeclaration Parent => (JobDeclaration)base.Parent;
		public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<CustomsStatusList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<JPMessageStatusList>();

		public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PaymentMethodCodeList>();

		public override CodeDescriptionPairList TransportTypeList => Factory.GetCachedValue<TransportTypeList>();

		public override ICodeDescriptionPairList CustomsOfficeList
		{
			get
			{
				var today = ZDateTime.Today;

				return Factory.GetCachedValue($"JPCustomsOfficeList-{today}", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, today));
					return result;
				});
			}
		}

		public override IBusinessObjectCollection CarrierCodeCollection
		{
			get
			{
				return Factory.GetCachedValue<IBusinessObjectCollection>($"JPCarrierCodeCollection-{ZDateTime.Today}-{Parent.JE_TransportMode}", () =>
				{
					if (Parent.IsAir)
					{
						return new RefAirlineCollection(Factory);
					}
					else
					{
						return new ZZRefCarrierCombinedCollection(Factory, Core.Constants.CountryCodes.Japan, ZString.Empty, Core.Constants.TransportModes.Sea)
						{
							FilterBusinessObjectDefaults =
							{
								new FilterBusinessObjectDefault("Country/Region or Grouping", "Property", (ZString)Core.Constants.CountryCodes.Japan, false),
								new FilterBusinessObjectDefault("Transport Mode", "Property", (ZString)Core.Constants.TransportModes.Sea, false)
							}
						};
					}
				});
			}
		}

		public override CodeDescriptionPairList VolumeUnitList => CustomsUnitOfMeasureList.GetVolumeUnitList(Factory);

		protected override CodeDescriptionPairList PackingUnitTypesListCore => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Japan, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, Parent.DateOfValuation);

		public CodeDescriptionPairList ValueTypeList => Factory.GetCachedValue<ValueTypeList>();

		public CodeDescriptionPairList CustomsOfficeDepartmentsList
		{
			get
			{
				var declaration = Parent;
				var today = ZDateTime.Today;

				return declaration == null || declaration.JE_CustomsOffice.Length < 2
					? new CodeDescriptionPairList()
					: Factory.GetCachedValue($"JPCustomsOfficeDepartment-{today}-{declaration.JE_CustomsOffice}", () =>
					{
						var filter = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.StartsWith, declaration.JE_CustomsOffice.SubstringSafe(0, 2));
						var departmentCodes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCustomsOfficeDepartment, today, filter).OrderBy(x => x.ZZD_Code);

						var result = new CodeDescriptionPairList();
						departmentCodes.ForEach(x => result.AddPairIfNotExist(x.ZZD_Code.SubstringSafe(2, 2), x.ZZD_Description));

						return result;
					});
			}
		}

		public IList<GlbExternalPasswordCUS> NACCSCredentialsList
		{
			get
			{
				var staff = Parent.CusAgent;
				if (staff != null)
				{
					return GlbStaffWrapper.Get(staff)
						.PasswordCollection
						.Where(pw => pw.GP_Transport == UserCodeSpecificTransportModeList.Codes.BTH || pw.GP_Transport == Parent.JE_TransportMode)
						.ToList();
				}
				else
				{
					return Enumerable.Empty<GlbExternalPasswordCUS>().ToList();
				}
			}
		}

		public CodeDescriptionPairList PaymentDeadlineExtensionCodeList => Factory.GetCachedValue<PaymentDeadlineExtensionCodeList>();

		public CodeDescriptionPairList ReceiptModeList => Factory.GetCachedValue<ReceiptModeList>();

		public CodeDescriptionPairList DeliveryModeList => Factory.GetCachedValue<DeliveryModeList>();

		public OrgHeaderCollection InspectionWitnessOrganisations => fInspectionWitnessOrganisations ??= GetFilteredOrganisations(Factory, OrgCusCode.JapanCodeTypes.NUC);
		OrgHeaderCollection fInspectionWitnessOrganisations;

		public IBusinessObjectCollection RadioCallSignVessels => RadioCallSignCodeFindBoxCollection.GetCachedCollection(Factory, Parent.JE_RadioCallSign);

		OrgHeaderCollection GetFilteredOrganisations(BusinessObjectFactory factory, string codeType)
		{
			var result = new OrgHeaderCollection(factory);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property1", (ZString)Core.Constants.CountryCodes.Japan, true));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Registration Country/Type", "Property2", (ZString)codeType, true));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property8", ZBool.True, true));
			return result;
		}
	}
}
