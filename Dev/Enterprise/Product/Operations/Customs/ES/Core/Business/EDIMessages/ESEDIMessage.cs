using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.EDIInterchanges;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.EDIMessages
{
	public class ESEDIMessage : EDIMessage, Integration.Customs.ES.IEDIMessage
	{
		public ESEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal ZString ESMessageNumberPlaceHolder => MessageNumberPlaceHolder;
		public const string ESExportAmendmentLocalReferenceNumberPlaceHolder = "<<EXPORT_AMENDMENT_LOCAL_REF_NUMBER_PLACE_HOLDER>>";

		public ZString BusinessObjectReference { get; set; }

		public new ESEDIInterchange Interchange => Factory.Load<ESEDIInterchange>(EM_EI);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.ESCustomsMessage;
		}

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.ESCustomsEDIFACTNumberFountain("M", ApplicationCodes.ESCustomsMessage).GetNextFormatted(Factory);

		protected override void PopulateMessageNumber()
		{
			base.PopulateMessageNumber();
			if (Interchange != null)
			{
				if (UsesPlaceHolders)
				{
					Interchange.EI_BodyText = Interchange.EI_BodyText.Replace(MessageNumberPlaceHolderOverride, EM_MessageNum);
					Interchange.EI_BodyText = Interchange.EI_BodyText.Replace(MessageNumberPlaceHolderHtml, EM_MessageNum);
				}

				Interchange.EI_HeaderText = Interchange.EI_HeaderText.Replace(MessageNumberPlaceHolderHtml, EM_MessageNum);
			}

			if (UsesPlaceHolders)
			{
				EM_MessageText = EM_MessageText.Replace(MessageNumberPlaceHolderHtml, EM_MessageNum);
			}
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(CusExitDetail);
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();

			if (EM_MessageType == DeclarationMessageTypeList.Codes.ExportAmendment)
			{
				var boRefNumber = BusinessObjectReference;
				var localReferenceNumberSuffix = Env.NumberFountains.ESBGMLocalReferenceSuffix(boRefNumber).GetNextFormatted(Factory);

				EM_MessageText = EM_MessageText
					.Replace(ESExportAmendmentLocalReferenceNumberPlaceHolder, boRefNumber + "_" + localReferenceNumberSuffix);
			}
		}

		protected override IStreamFormatter MessageStreamFormatter
		{
			get
			{
				return new ESEDIMessageStreamFormatter();
			}
		}
	}
}
