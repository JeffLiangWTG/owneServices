using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	class DrpCreator : CusAwbToInventoryMessageGenerator
	{
		public DrpCreator(DropOffWrapper wrapper, JobDeclaration decForMessaging)
			: base(decForMessaging.Factory)
		{
			this.decForMessaging = decForMessaging;
			MessageFunction = new CcsukTransmissionMessageFunction.CIM.DRP(wrapper);
			this.cimWrapper = new CusAwbToCimWrapper(decForMessaging.Factory, MessageFunction);
		}

		public override string MakeMessageText()
		{
			var edifactWithPlaceholders = cimWrapper.MakeMessageText();
			var rotationNumber = 0;
			lock (this)
			{
				var rotationNumberString = GBCustomsDataRegistry.Instance.CcsukDropOffRotationNumberHighWatermark.Value;
				rotationNumber = ParseRotationNumberString(rotationNumberString);
				rotationNumber += 1;
				SaveNewRotationNumber(rotationNumber);
			}

			return edifactWithPlaceholders.Replace(DropOffHeader.RotationNumberPlaceholder, rotationNumber.ToString("000#"));
		}

		int ParseRotationNumberString(string rotationNumberString)
		{
			var rotationNumber = 0;
			if (!string.IsNullOrEmpty(rotationNumberString))
			{
				var parts = (rotationNumberString + "~").Split('~');
				var dateStamp = DateTime.MinValue;
				DateTime.TryParse(parts[0], out dateStamp);
				if (dateStamp.Day <= ZDateTime.Today.Day - 1)
				{
					// reset
				}
				else
				{
					int.TryParse(parts[1], out rotationNumber);
				}
			}
			return rotationNumber;
		}

		void SaveNewRotationNumber(int rotationNumber)
		{
			GBCustomsDataRegistry.Instance.CcsukDropOffRotationNumberHighWatermark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime().ToLongDateString() + "~" + rotationNumber.ToString());
		}

		protected override EDIMessage GetNewMessageCore()
		{
			return decForMessaging.Messages.AddNew();
		}

		public override ZString MessageInterpretation
		{
			get { return cimWrapper.MessageInterpretation; }
		}

		protected override ZString SenderPimaCore
		{
			get
			{
				var cred = CredentialsSetting.GetCredentialsForBadge(decForMessaging.JE_CustomsProfile, decForMessaging.Company.PK);
				return cred?.PIMA ?? ZString.Empty;
			}
		}

		public override ZString RecipientPima
		{
			get { return cimWrapper.GetRecipientPimaForAirportAndShed(decForMessaging.JE_LocationOfGoods + decForMessaging.SubLocation); }
		}

		readonly CusAwbToCimWrapper cimWrapper;
		internal CcsukTransmissionMessageFunction MessageFunction { get; private set; }
		readonly JobDeclaration decForMessaging;
	}
}
