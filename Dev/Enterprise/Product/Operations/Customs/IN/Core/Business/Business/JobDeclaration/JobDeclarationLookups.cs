using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public CodeDescriptionPairList OriginStateList => Factory.GetStateList(Core.Constants.CountryCodes.India, false);

	public override CodeDescriptionPairList CargoIdTypeList => Factory.GetCachedValue<Common.IN.INContainerModeList>();

	public CodeDescriptionPairList EPZCodeList => Factory.GetCachedValue<EPZCodeList>();

	public CodeDescriptionPairList ExporterTypeList => Factory.GetCachedValue<ExporterTypeList>();

	public new ICollection CustomsOfficeList => UniversalReferenceDataHelper.GetCustomsOfficeCollection(Factory, Parent.IsAir, Parent.DateOfValuation);

	public CodeDescriptionPairList SealByCodeList => Factory.GetCachedValue<SealByCodeList>();

	public CodeDescriptionPairList StuffingAtList => Factory.GetCachedValue<StuffingAtList>();

	public CodeDescriptionPairList SampleAccompaniedList => Factory.GetCachedValue<YesNoList>();

	public CodeDescriptionPairList SampleForwardedList => Factory.GetCachedValue<YesNoList>();

	public CodeDescriptionPairList VerifiedList => Factory.GetCachedValue<YesNoList>();

	public OrganisationsFindBoxCollection ExportOrientedUnitsCollection
	{
		get
		{
			if (fExportOrientedUnitsCollection == null)
			{
				fExportOrientedUnitsCollection = new OrganisationsFindBoxCollection(Factory);
				var filterBusinessObjectDefaults = fExportOrientedUnitsCollection.FilterBusinessObjectDefaults;
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property3", ZBool.True));
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country/Region", "Property", (ZString)Core.Constants.CountryCodes.India));
			}
			return fExportOrientedUnitsCollection;
		}
	}
	OrganisationsFindBoxCollection fExportOrientedUnitsCollection;

	public OrganisationsFindBoxCollection TranshipperCollection
	{
		get
		{
			if (transhipperCollection == null)
			{
				transhipperCollection = new OrganisationsFindBoxCollection(Factory);
				var filterBusinessObjectDefaults = transhipperCollection.FilterBusinessObjectDefaults;
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Secondary Type", "Property", OrganisationSecondaryTypes.LocalTransport));
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property4", ZBool.True));
				filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country/Region", "Property", (ZString)Core.Constants.CountryCodes.India));
			}
			return transhipperCollection;
		}
	}
	OrganisationsFindBoxCollection transhipperCollection;
}
