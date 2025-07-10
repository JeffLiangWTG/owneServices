using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.Messaging
{
	[CodeAlive("To be used by CA Customs")]
	class CCSEntryFunctionalExceptionMessageBuilder : D99BMessageBuilder<ICCSEntryFunctionalExceptionTypeX, CUSRESMessage, EDIMessage>
	{
		public CCSEntryFunctionalExceptionMessageBuilder(ICCSEntryFunctionalExceptionTypeX data, MessageSubTypes messageSubType)
			: base(data, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage()
		{
			CreateUNH();
			CreateBGM();
			CreateDTM();
			CreateGIS();
			CreateGroup4();
		}

		void CreateUNH()
		{
			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, MessageTypeList.CustomsResponseMessage, "S", "99B", ControllingAgencyList.UnCefact);
		}

		void CreateBGM()
		{
			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateBGM(bgm, data.InboundB3TransactionNumber, ZString.Empty, MessageFunctionCodeList.Response);
		}

		void CreateDTM()
		{
			var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateDTM(dtm, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime, ZDateTime.Now, "yyyyMMddHHmm", DateTimePeriodFormatCodeList.Ccyymmddhhmm);
		}

		void CreateGIS()
		{
			var gis = edifactMessage.GIS.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateGIS(gis, ProcessingIndicatorDescriptionCodeList.ErrorMessage);
		}

		void CreateGroup4()
		{
			var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();
			var erp = group4.ERP.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateERP(erp, MessageSectionCodedList.DetailSectionOfAMessage, EDIMessage.MessageNumberPlaceHolder, "29");

			foreach (var errFtx in data.ErrorFtxs)
			{
				var ftx = group4.FTX.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateFTX(ftx, TextSubjectCodeQualifierList.ErrorDescriptionFreeText, errFtx.ErrorText1, errFtx.ErrorText2);
			}
		}
	}
}
