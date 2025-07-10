using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class CommonPreviousDocument : EU.NCTS.Business.CommonPreviousDocument
{
	public CommonPreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusSupportingInfoValidation GetNewValidation() => new CommonPreviousDocumentValidation(this);

	#region N785Format
	public ZString CSI_ReferenceNumberN785Pos1
	{
		get => CSI_ReferenceNumber.SubstringSafe(0, 1);
		set => MakeReferenceNumber(value, CSI_ReferenceNumberN785Pos2To7, CSI_ReferenceNumberN785Pos8, CSI_ReferenceNumberN785Pos9To15, CSI_ReferenceNumberN785Pos16, CSI_ReferenceNumberN785Pos17To20);
	}

	public ZWrappedPropertyInfo CSI_ReferenceNumberN785Pos1Info => GetWrappedZPropertyInfo(nameof(CSI_ReferenceNumberN785Pos1), x => CSI_ReferenceNumberInfo);

	public ZString CSI_ReferenceNumberN785Pos2To7
	{
		get => CSI_ReferenceNumber.SubstringSafe(1, 6);
		set => MakeReferenceNumber(CSI_ReferenceNumberN785Pos1, value, CSI_ReferenceNumberN785Pos8, CSI_ReferenceNumberN785Pos9To15, CSI_ReferenceNumberN785Pos16, CSI_ReferenceNumberN785Pos17To20);
	}

	public ZWrappedPropertyInfo CSI_ReferenceNumberN785Pos2To7Info => GetWrappedZPropertyInfo(nameof(CSI_ReferenceNumberN785Pos2To7), x => CSI_ReferenceNumberInfo);

	public ZString CSI_ReferenceNumberN785Pos8 => "L";

	public ZString CSI_ReferenceNumberN785Pos9To15
	{
		get => CSI_ReferenceNumber.SubstringSafe(8, 7);
		set => MakeReferenceNumber(CSI_ReferenceNumberN785Pos1, CSI_ReferenceNumberN785Pos2To7, CSI_ReferenceNumberN785Pos8, value, CSI_ReferenceNumberN785Pos16, CSI_ReferenceNumberN785Pos17To20);
	}

	public ZWrappedPropertyInfo CSI_ReferenceNumberN785Pos9To15Info => GetWrappedZPropertyInfo(nameof(CSI_ReferenceNumberN785Pos9To15), x => CSI_ReferenceNumberInfo);

	public ZString CSI_ReferenceNumberN785Pos16 => "*";

	public ZString CSI_ReferenceNumberN785Pos17To20
	{
		get => CSI_ReferenceNumber.SubstringSafe(16, 4);
		set => MakeReferenceNumber(CSI_ReferenceNumberN785Pos1, CSI_ReferenceNumberN785Pos2To7, CSI_ReferenceNumberN785Pos8, CSI_ReferenceNumberN785Pos9To15, CSI_ReferenceNumberN785Pos16, value);
	}

	public ZWrappedPropertyInfo CSI_ReferenceNumberN785Pos17To20Info => GetWrappedZPropertyInfo(nameof(CSI_ReferenceNumberN785Pos9To15), x => CSI_ReferenceNumberInfo);

	void MakeReferenceNumber(ZString part1, ZString part2, ZString part3, ZString part4, ZString part5, ZString part6)
	{
		var builder = new ZStringBuilder();

		if (part1.IsEmpty)
		{
			part1 = " ";
		}

		if (part2.IsEmpty)
		{
			part2 = "      ";
		}

		if (part4.IsEmpty)
		{
			part4 = "       ";
		}

		if (part6.IsEmpty)
		{
			part6 = "    ";
		}

		builder.Append(part1);
		builder.Append(part2);
		builder.Append(part3);
		builder.Append(part4);
		builder.Append(part5);
		builder.Append(part6);

		CSI_ReferenceNumber = builder.ToString();
	}
	#endregion
}
