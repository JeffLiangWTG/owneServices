using System;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class BaseTransitDataProviderTest<TDataProvider, TMessageSendingObject> : TestCaseWithFactory
	where TDataProvider : class
	where TMessageSendingObject : class, INctsMessageSendingObject
{
	protected abstract string MovementType { get; }

	protected NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(MovementType);
		return nctsHeader;
	}

	protected NctsBill NctsBill => nctsBill ?? (nctsBill = CreateNctsBill());
	NctsBill nctsBill;

	NctsBill CreateNctsBill()
	{
		var nctsBill = NctsHeader.Bills.AddNew();
		AddGoodsItem(nctsBill);
		return nctsBill;
	}

	protected virtual void AddGoodsItem(NctsBill bill)
	{
	}

	protected NctsDepartureMovementHeader DepartureMovementHeader => NctsHeader.MovementHeader;
	protected NctsDepartureCargoDesc DepartureGoodsItem => NctsBill.GoodsItems.FirstOrDefault() ?? NctsBill.GoodsItems.AddNew();

	protected NctsArrivalMovementHeader ArrivalMovementHeader => NctsHeader.ArrivalMovementHeader;
	protected NctsArrivalCargoDesc ArrivalGoodsItem => NctsBill.ArrivalGoodsItems.FirstOrDefault() ?? NctsBill.ArrivalGoodsItems.AddNew();

	protected TMessageSendingObject MessageSendingObject => messageSendingObject ?? (messageSendingObject = CreateMessageSendingObject());
	TMessageSendingObject messageSendingObject;

	protected TMessageSendingObject CreateMessageSendingObject() => (TMessageSendingObject)Activator.CreateInstance(typeof(TMessageSendingObject), NctsHeader);

	protected TDataProvider DataProvider => dataProvider ?? (dataProvider = CreateDataProvider());
	TDataProvider dataProvider;

	protected abstract TDataProvider CreateDataProvider();

	protected void ResetDataProvider() => dataProvider = null;

	protected void AssertParticipentIdentificationNumberOrNameAddress(string assertionMessage, JobDocAddress docAddress, Func<IParticipant> participient, string regNoType)
	{
		docAddress.E2_AddressOverride = false;
		docAddress.Address.CompanyName = "name";

		ResetDataProvider();
		docAddress.Organisation.CustomsCodes.RemoveAll();
		AssertNull($"{assertionMessage}: No IdentificationNumber: IdentificationNumber", participient().IdentificationNumber);
		AssertNotNull($"{assertionMessage}: No IdentificationNumber: Name", participient().Name);
		AssertNotNull($"{assertionMessage}: No IdentificationNumber: Address", participient().Address);

		ResetDataProvider();
		docAddress.Organisation.CustomsCodes.AddNew(regNoType, "123");
		AssertNotNull($"{assertionMessage}: With IdentificationNumber: IdentificationNumber", participient().IdentificationNumber);
		AssertNull($"{assertionMessage}: With IdentificationNumber: Name", participient().Name);
		AssertNull($"{assertionMessage}: With IdentificationNumber: Address", participient().Address);
	}
}
