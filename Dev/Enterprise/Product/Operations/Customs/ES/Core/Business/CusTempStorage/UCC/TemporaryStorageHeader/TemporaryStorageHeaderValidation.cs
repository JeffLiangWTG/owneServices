using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageHeaderValidation : EU.Business.CusTempStorage.TemporaryStorageHeaderValidation
{
	public TemporaryStorageHeaderValidation(TemporaryStorageHeader parent) : base(parent)
	{
	}

	public new TemporaryStorageHeader Parent => (TemporaryStorageHeader)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();

		ValidateDestinationCustomsOffice();
		ValidateDestinationGoodsLocationDescription();
		ValidateDsdtMrnNumber();
	}

	protected override bool HasEORINumber(OrgAddress orgAddressExpectEORI) => !orgAddressExpectEORI?.Header?.GetIDCode().IsEmpty ?? false;

	protected override void CheckAMA_OA_Carrier() { }

	protected override void CheckAMA_CustomsProfile()
	{
		base.CheckAMA_CustomsProfile();

		var header = Parent;
		CertificateHelper.CheckCustomsProfile(header.AMA_CustomsProfileInfo, header.AMA_CustomsProfile, header.CustomsAgent);
	}

	protected override void CheckAMA_TransportMode()
	{
		base.CheckAMA_TransportMode();

		var header = Parent;
		if (header.IsMessageTypeG5V1Reception || header.IsMessageTypeG5V1Expedition)
		{
			MandatoryValidation.MessageErrorIfNotEntered(header.AMA_TransportModeInfo);
		}
	}

	protected override void CheckTransportType()
	{
		base.CheckTransportType();

		var header = Parent;
		if (header.IsMessageTypeG5V1Reception || header.IsMessageTypeG5V1Expedition)
		{
			MandatoryValidation.MessageErrorIfNotEntered(header.TransportTypeInfo);
		}
	}

	protected override void CheckAMA_MessageType()
	{
		base.CheckAMA_MessageType();

		var header = Parent;
		if (header.IsMessageTypeG5_LAM_TSM)
		{
			var bill = header.Bills.FirstOrDefault();
			if (bill == null || bill.Packs.Count == 0)
			{
				header.AMA_MessageTypeInfo.AddMessageError(Res.GetString("8022EDA9-A7FF-43BA-8247-F62AAE73B5C2", "You have not entered a Pack."));
			}

			var items = bill?.PackedItems.Count ?? 0;

			if (bill == null || items == 0)
			{
				header.AMA_MessageTypeInfo.AddMessageError(Res.GetString("F74D7016-BF22-4CE8-B79B-89CCF29D7C27", "You have not entered an Item."));
			}

			if (header.IsMessageTypeG5V1Reception || header.IsMessageTypeG5V1Expedition)
			{
				var prevDocsBill = bill?.PreviousDocuments.Count ?? 0;
				var prevDocsItems = bill?.PackedItems?.Where(i => i.PreviousDocuments.Count == 0).Count() ?? 0;

				if (bill == null || (prevDocsBill == 0 && (items == 0 || prevDocsItems != 0)))
				{
					header.AMA_MessageTypeInfo.AddMessageError(Res.GetString("3CD41E22-0472-4AE4-9992-A0385CE4DD6C", "You have not entered a Previous Document."));
				}
			}
		}
	}

	#region New Validations

	public void ValidateDestinationCustomsOffice() => ValidateCalculatedProperty(Parent.DestinationCustomsOfficeInfo);

	protected void CheckDestinationCustomsOffice()
	{
		var header = Parent;
		if (header.IsMessageTypeG5V1Reception || header.IsMessageTypeG5V1Expedition)
		{
			MandatoryValidation.MessageErrorIfNotEntered(header.DestinationCustomsOfficeInfo);
		}
	}

	public void ValidateDestinationGoodsLocationDescription() => ValidateCalculatedProperty(Parent.DestinationGoodsLocationDescriptionInfo);

	protected void CheckDestinationGoodsLocationDescription()
	{
		var header = Parent;
		if (header.IsMessageTypeG5_LAM_TSM)
		{
			MandatoryValidation.MessageErrorIfNotEntered(header.DestinationGoodsLocationDescriptionInfo);
		}
	}

	public void ValidateDsdtMrnNumber() => ValidateCalculatedProperty(Parent.DsdtMrnNumberInfo);

	protected void CheckDsdtMrnNumber()
	{
		var header = Parent;
		if (header.IsMessageTypeTSM && !header.UnionGoods && header.DsdtMrnNumber.IsEmpty)
		{
			header.DsdtMrnNumberInfo.AddMessageError(Res.GetString("BD4DCDD3-14CD-44B1-A196-C893D4E82214", "You have not entered a DSDT Number. Please enter DSDT (SD Format) in the maritime format."));
		}
	}

	#endregion
}
