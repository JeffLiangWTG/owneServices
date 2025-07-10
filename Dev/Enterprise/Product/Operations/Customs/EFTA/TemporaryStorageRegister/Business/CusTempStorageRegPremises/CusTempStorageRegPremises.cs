using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

[SingleObjectAroundARow]
[CodeProperty(CusTempStorageRegPremises.Schema.SRP_CustomsLocation), DescriptionProperty(CusTempStorageRegPremises.Schema.SRP_Code)]
public class CusTempStorageRegPremises : AutoCusTempStorageRegPremises
		, ICusTempStorageRegPremises
		, ICustomsNumberViewStmNumsParent
		, IEDocsProvider
{
	public CusTempStorageRegPremises(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : AutoCusTempStorageRegPremises.Schema
	{
		public const string TypeDescription = nameof(CusTempStorageRegPremises.TypeDescription);
	}

	[ResourceStringData("CBDFB078-7AA8-4D84-BAFE-062321BBC6D8", Caption = "Code")]
	public override ZString SRP_Code
	{
		get => base.SRP_Code;
		set => base.SRP_Code = value;
	}

	[ResourceStringData("9988C959-07A6-4823-9DFB-6A8FB10992CE", Caption = "Type")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegPremisesLookups.TypeList))]
	public override ZString SRP_Type
	{
		get => base.SRP_Type;
		set => base.SRP_Type = value;
	}

	[ResourceStringData("2B7C2607-21F7-46D5-BBD7-BD861F1BEBDC", Caption = "Description", MediumCaption = "Description", ShortCaption = "Desc.", FullDescription = "Description for Premises")]
	public override ZString SRP_Description { get => base.SRP_Description; set => base.SRP_Description = value; }

	[ResourceStringData("A06A40DF-0BC2-4F83-B0C2-4BA5B9A431F0", Caption = "Address", MediumCaption = "Address", ShortCaption = "Addr.", FullDescription = "Address for Premises")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegPremisesLookups.PremisesAddressList))]
	public override ZGuid SRP_OA_PremisesAddress { get => base.SRP_OA_PremisesAddress; set => base.SRP_OA_PremisesAddress = value; }

	[ResourceStringData("A0198ED2-908F-4E3C-A6B5-EF3C0EECB93D", Caption = "Type Description")]
	public virtual ZString TypeDescription => GetTypeDescription(SRP_Type) ?? ZString.Empty;

	protected virtual string GetTypeDescription(string type)
		=> Factory.GetCachedValue("TemporaryStorage|CusTempStorageRegPremises|TypeDescription|" + type, () => Lookups.TypeList.GetDescriptionFromCode(type));

	[ResourceStringData("C417ACF8-85B7-4FA9-9E1D-61BC8F87F838", Caption = "Location")]
	[List(nameof(Lookups) + "." + nameof(CusTempStorageRegPremisesLookups.CustomsLocationList))]
	public override ZString SRP_CustomsLocation
	{
		get => base.SRP_CustomsLocation;
		set => base.SRP_CustomsLocation = value;
	}

	public ZString LayoutProviderKey => LayoutProviderKeyCore;

	protected virtual ZString LayoutProviderKeyCore => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	protected override ZString HumanReadableNameCore => Res.GetString("348A765F-2500-4B8D-AD1F-ABDB18FC7EE2", "Code {0}", SRP_Code);

	protected override AutologState AutoLoggingState => AutologState.AutoLogged;

	CustomsNumberViewStmNumsBusinessProvider ICustomsNumberViewStmNumsParent.CustomsNumberProvider => NumberProvider;

	public TSCustomsNumberViewStmNumsBusinessProvider NumberProvider
	{
		get
		{
			var providerKey = CustomsNumberProviderKey;
			if (numberProvider == null || numberProvider.ProviderKey != providerKey)
			{
				numberProvider = NumberProviderFactory.GetProvider(providerKey, PK);
			}
			return numberProvider;
		}
	}
	TSCustomsNumberViewStmNumsBusinessProvider numberProvider;

	public string CustomsNumberProviderKey => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

	protected TSCustomsNumberViewStmNumsBusinessProviderFactory NumberProviderFactory => numberProviderFactory ??= CreateTSCustomsNumberViewStmNumsBusinessProviderFactory();
	TSCustomsNumberViewStmNumsBusinessProviderFactory numberProviderFactory;

	protected virtual TSCustomsNumberViewStmNumsBusinessProviderFactory CreateTSCustomsNumberViewStmNumsBusinessProviderFactory() => new(Factory);

	#region IEDocsProvider Members

	public EDocsProviderSupporter GetEDocsProviderSupporter() => new (this);

	public DocumentSupporter DocumentSupporter => documentSupporter ??= GetNewDocumentSupporter();
	CusTempStorageRegPremisesDocumentSupporter documentSupporter;

	protected virtual CusTempStorageRegPremisesDocumentSupporter GetNewDocumentSupporter() => new (this);

	public DocManagerInfo DocManagerInfo => docManagerInfo ??= GetNewDocManagerInfo();
	DocManagerInfo docManagerInfo;

	protected virtual DocManagerInfo GetNewDocManagerInfo() => new (this, Core.Constants.DocManagerCodes.TempStorageRegPremises);

	#endregion
}
