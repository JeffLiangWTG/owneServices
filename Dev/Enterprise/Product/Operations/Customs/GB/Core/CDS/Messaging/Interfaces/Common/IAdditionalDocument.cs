using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IAdditionalDocument
	{
		ZString CategoryCode { get; }
		ZString TypeCode { get; }
		ZString ID { get; }
		ZString LPCOExemptionCode { get; }
		ZString Name { get; }
		ZDateTime EffectiveDateTime { get; }
		ZString Submitter { get; }
		IWriteOff WriteOff { get; }
		ZDateTime SystemCreateTime { get; }
	}

	class AdditionalDocumentWrapper : Document, IAdditionalDocument, IEquatable<AdditionalDocumentWrapper>
	{
		AdditionalDocumentWrapper(SupportingDocument supportingDocument) : base(supportingDocument)
		{
		}

		public static AdditionalDocumentWrapper New(SupportingDocument supportingDocument)
		{
			return new AdditionalDocumentWrapper(supportingDocument);
		}

		ZString IAdditionalDocument.CategoryCode => ((ISupportingDocument)this).Code.SubstringSafe(0, 1);

		ZString IAdditionalDocument.TypeCode => ((ISupportingDocument)this).Code.SubstringSafe(1);

		ZString IAdditionalDocument.ID
		{
			get
			{
				var result = new ZStringBuilder().AppendIfNotEmpty(((ISupportingDocument)this).Reference)
					.AppendIfNotEmpty(((ISupportingDocument)this).Part)
					.ToStringWithDelimiterBetweenAppends("-");
				return ((ZString)result).StripNewlineCharacters(CDSDataElementsLengths.AdditionalDocumentIdMaxLength);
			}
		}

		ZString IAdditionalDocument.LPCOExemptionCode => document.CSI_Status;

		ZString IAdditionalDocument.Name => document.CSI_Description.StripNewlineCharacters(CDSDataElementsLengths.AdditionalDocumentNameMaxLength);

		ZDateTime IAdditionalDocument.EffectiveDateTime => document.CSI_DateOfIssue;

		ZString IAdditionalDocument.Submitter => document.CSI_ReferenceNumber2.StripNewlineCharacters(CDSDataElementsLengths.AdditionalDocumentSubmitterNameMaxLength);

		IWriteOff IAdditionalDocument.WriteOff
		{
			get
			{
				var quantityUnitCode = ZString.Empty;
				var suppDoc = document;
				var suppDocQuantity = suppDoc.CSI_Quantity;
				var suppDocUnitOfQuantity = suppDoc.CSI_UnitOfQuantity;
				var suppDocUnitOfQuantity2 = suppDoc.CSI_UnitOfQuantity2;

				if (!suppDocUnitOfQuantity.IsEmpty)
				{
					if (suppDocUnitOfQuantity2.IsEmpty)
					{
						quantityUnitCode = suppDocUnitOfQuantity;
					}
					else
					{
						quantityUnitCode = ZString.Join("#", new ZString[] { suppDocUnitOfQuantity, suppDocUnitOfQuantity2 });
					}
				}
				var quantity = suppDocQuantity.IsEmpty ? ZDecimal.Zero : suppDocQuantity;
				return WriteOffWrapper.New(quantity, new ZDecimal(null), quantityUnitCode);
			}
		}

		bool IEquatable<AdditionalDocumentWrapper>.Equals(AdditionalDocumentWrapper other)
		{
			if (other == null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			var thisWrapper = (IAdditionalDocument)this;
			var otherWrapper = (IAdditionalDocument)other;

			return thisWrapper.CategoryCode == otherWrapper.CategoryCode
					&& thisWrapper.TypeCode == otherWrapper.TypeCode
					&& thisWrapper.ID == otherWrapper.ID
					&& thisWrapper.LPCOExemptionCode == otherWrapper.LPCOExemptionCode
					&& thisWrapper.Name == otherWrapper.Name
					&& thisWrapper.EffectiveDateTime == otherWrapper.EffectiveDateTime
					&& thisWrapper.Submitter == otherWrapper.Submitter
					&& thisWrapper.WriteOff?.AmountAmount == otherWrapper.WriteOff?.AmountAmount
					&& thisWrapper.WriteOff?.QuantityQuantity == otherWrapper.WriteOff?.QuantityQuantity
					&& thisWrapper.WriteOff?.QuantityQuantityUnitCode == otherWrapper.WriteOff?.QuantityQuantityUnitCode
					&& thisWrapper.SystemCreateTime == otherWrapper.SystemCreateTime;
		}

		public override int GetHashCode()
		{
			var thisWrapper = (IAdditionalDocument)this;

			var writeOff = thisWrapper.WriteOff;
			var hashWriteOff = writeOff == null
				? 0
				: writeOff.AmountAmount.GetHashCode()
					^ writeOff.QuantityQuantity.GetHashCode()
					^ writeOff.QuantityQuantityUnitCode.GetHashCode();

			return thisWrapper.CategoryCode.GetHashCode()
					^ thisWrapper.TypeCode.GetHashCode()
					^ thisWrapper.ID.GetHashCode()
					^ thisWrapper.LPCOExemptionCode.GetHashCode()
					^ thisWrapper.Name.GetHashCode()
					^ thisWrapper.EffectiveDateTime.GetHashCode()
					^ thisWrapper.Submitter.GetHashCode()
					^ hashWriteOff
					^ thisWrapper.SystemCreateTime.GetHashCode();
		}

		public ZDateTime SystemCreateTime => document.CSI_SystemCreateTimeUtc;
	}
}
