using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INCTSPrettierData
	{
		public void FillSharedFields(NCTSPrettierSharedFields sharedFields);
		IReadOnlyCollection<INCTSPrettierAdditionalBlock> AdditionalBlocks { get; }
	}

	public interface INCTSPrettierAdditionalBlock
	{ }

	public interface INCTSPrettierTable : INCTSPrettierAdditionalBlock
	{
		ZString Border { get; }
		ZString Width { get; }

		ZString Caption { get; }
		IReadOnlyCollection<(ZString Caption, ZString Attributes)> Columns { get; }
		IReadOnlyCollection<(ZString Key, ZString Value)> AdditionalInfo { get; }
	}

	public interface INCTSPrettierGuaranteeData
	{
		ZString Type { get; }
		ZString GRN { get; }
		ZString OtherNumber { get; }
		ZString Amount { get; }
		ZString Currency { get; }
	}

	public interface INCTSPrettierConsignmentData
	{
		ZString UCRReference { get; }
		IReadOnlyCollection<INCTSPrettierGoodsItemData> GoodsItems { get; }
	}

	public interface INCTSPrettierGoodsItemData
	{
		ZString ItemNumber { get; }
		ZString UCRReference { get; }
		ZString Description { get; }
		ZString HarmonizedSubHeadingCode { get; }
	}

	public interface INCTSAddressData
	{
		ZString CareOf { get; }

		ZString City { get; }

		ZString Country { get; }

		ZString Postcode { get; }

		ZString StreetAndNumber { get; }
	}

	public interface INCTSTranshipmentData
	{
		bool ContainerIndicator { get; }

		ZString TransportMeansNationality { get; }

		ZString TransportMeansIdentificationNumber { get; }

		ZString TransportMeansTypeOfIdentification { get; }
	}

	public interface INCTSIncidentData
	{
		int SequenceNumber { get; }

		int Code { get; }

		ZString Text { get; }

		DateTime? EndorsementDate { get; }

		ZString EndorsementAuthority { get; }

		ZString EndorsementPlace { get; }

		ZString EndorsementCountry { get; }

		ZString LocationQualifierOfIdentification { get; }

		ZString LocationUNLocode { get; }

		ZString LocationCountry { get; }

		ZString LocationLatitude { get; }

		ZString LocationLongitude { get; }

		INCTSAddressData LocationAddress { get; }

		IReadOnlyCollection<INCTSTransportEquipment> TransportEquipments { get; }

		INCTSTranshipmentData Transhipment { get; }
	}

	public interface INCTSSeal
	{
		int SequenceNumber { get; }

		string Identifier { get; }
	}

	public interface INCTSTransportEquipment
	{
		int SequenceNumber { get; }

		string ContainerIdentificationNumber { get; }

		int? NumberOfSeals { get; }

		IReadOnlyCollection<INCTSSeal> Seals { get; }

		IReadOnlyCollection<INCTSGoodsReference> GoodsReferences { get; }
	}

	public interface INCTSGoodsReference
	{
		int SequenceNumber { get; }

		int DeclarationGoodsItemNumber { get; }
	}
}
