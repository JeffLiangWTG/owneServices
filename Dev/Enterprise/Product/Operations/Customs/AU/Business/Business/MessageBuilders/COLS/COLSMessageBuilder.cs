using System.Text.Json;
using System.Text.Json.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class COLSMessageBuilder<T>
	{
		public COLSMessageBuilder(QuarantineColsHeader colsHeader)
		{
			this.colsHeader = Argument.NotNull(colsHeader, nameof(QuarantineColsHeader));
		}
		protected readonly QuarantineColsHeader colsHeader;

		public COLSMessage CreateNewMessage()
		{
			var message = colsHeader.Factory.New<COLSMessage>();
			message.EM_MessageType = MessageType;
			message.EM_MessageText = JsonSerializer.Serialize(GetMessageData(), new JsonSerializerOptions
			{
				DefaultIgnoreCondition = IgnoreNullProperties ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never
			});
			message.EM_LinkedObject = MessageParent;
			DoAdditionalProcessing(message);
			return message;
		}

		protected virtual void DoAdditionalProcessing(COLSMessage message)
		{
		}

		protected ZString GetLocalFormattedPhoneNumber(ZString phoneNumber)
		{
			var phoneNumberFormatter = new PhoneNumberFormatterAndValidator();
			var phoneNum = phoneNumberFormatter.FormatLocal(phoneNumber, Core.Constants.CountryCodes.Australia).KeepNumericCharacters();
			return phoneNum;
		}

		protected virtual ZBool IgnoreNullProperties => false;

		protected abstract ZString MessageType { get; }

		protected abstract BusinessObject MessageParent { get; }

		protected abstract T GetMessageData();
	}
}
