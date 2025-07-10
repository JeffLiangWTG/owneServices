using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business;

[XmlSerializerAssembly("Enterprise.Customs.BE.Business.XmlSerializers")]
public class CustomsRegistry : AutoCustomsRegistry
{
	public CustomsRegistry()
	{
	}

	public CustomsRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	[List(nameof(Organizations))]
	[RelatedBusinessObject(nameof(OrgParty))]
	public override ZGuid Organization
	{
		get => base.Organization;
		set => base.Organization = value;
	}

	[BusinessObjectTestExclude]
	public override ZInt CurrentNo
	{
		get => getCurrentNoFromNumberFountain();
		set => base.CurrentNo = value;
	}

	public OrgHeader OrgParty => CurrentFactory.Load<OrgHeader>(Organization);

	public OrgHeaderCollection Organizations => new OrgHeaderCollection(CurrentFactory);

	public override void ValidateStartingNo()
	{
		base.ValidateStartingNo();
		MandatoryValidation.CheckNotNegative(StartingNoInfo);
		if (DeclarationType.EqualsIgnoringCase(BERegistryDeclarationTypeList.Codes.AllTransitDepartureDeclarations))
		{
			MandatoryValidation.CheckNotZero(StartingNoInfo);
		}
	}

	[List(nameof(DeclarationTypes))]
	public override ZString DeclarationType { get => base.DeclarationType; set => base.DeclarationType = value; }

	public override void ValidateDeclarationType()
	{
		base.ValidateDeclarationType();
		ListValidation.ErrorIfInvalidCode(DeclarationTypeInfo, DeclarationTypes);
		if (!DeclarationType_ReadOnly &&
			DeclarationType.EqualsIgnoringCase(BERegistryDeclarationTypeList.Codes.AllTransitDepartureDeclarations)
			&& this.GetParentCollection(this, typeof(CustomsRegistryCollection)) is CustomsRegistryCollection customsRegistryCollection
			&& customsRegistryCollection.Cast<CustomsRegistry>().Any(x => x.PK != this.PK && x.Organization.Equals(Organization) && x.DeclarationType.EqualsIgnoringCase(BERegistryDeclarationTypeList.Codes.AllTransitDepartureDeclarations) && x.StartingDate > StartingDate))
		{
			DeclarationTypeInfo.AddError(Res.GetString("9D6AB633-1851-496A-92A0-0DB417323631", "There already exists a registry with declaration type DA and a starting date later then the date filled. Cancel the registration or fill a starting date that is later."));
		}
	}

	public BERegistryDeclarationTypeList DeclarationTypes => CurrentFactory.GetCachedValue<BERegistryDeclarationTypeList>();

	protected override bool Organization_ReadOnly => CurrentNo > 0 && !StartingDate.IsEmpty;

	protected override bool DeclarationType_ReadOnly => CurrentNo > 0 && !StartingDate.IsEmpty;

	protected override bool StartingNo_ReadOnly => CurrentNo > 0 && !StartingDate.IsEmpty;

	protected override bool StartingDate_ReadOnly => CurrentNo > 0 && !StartingDate.IsEmpty;

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.Organization, Organization.ToString());
		writer.WriteElementString(Schema.DeclarationType, DeclarationType);
		writer.WriteElementString(Schema.StartingNo, StartingNo.ToString());
		writer.WriteElementString(Schema.CurrentNo, CurrentNo.ToString());
		writer.WriteElementString(Schema.StartingDate, StartingDate.ToBestReadableDateString());
	}

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		Organization = ZGuid.TryParse(reader.ReadElementString(Schema.Organization), out var orgPKValue) ? orgPKValue : ZGuid.Empty;
		DeclarationType = reader.ReadElementString(Schema.DeclarationType);
		StartingNo = reader.ReadElementStringAsZInt(Schema.StartingNo);
		CurrentNo = reader.ReadElementStringAsZInt(Schema.CurrentNo);
		StartingDate = reader.ReadElementStringAsZDateTime(Schema.StartingDate, ZDateTime.BestReadableDateFormat);
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CustomsRegistry(fallbackLevel, factory);

	ZInt getCurrentNoFromNumberFountain()
	{
		var nextNumber = Organization.IsEmpty || StartingNo == 0 ? "1" : Env.NumberFountains.BECustomsRegistryNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid(), Organization.ToGuid(), DeclarationType, StartingDate, StartingNo).PeekPreliminaryFormatted(CurrentFactory);
		return int.Parse(nextNumber) - 1;
	}
}
