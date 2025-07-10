using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.Messaging
{
	[CodeAlive("To be used by CA Customs")]
	public class CCSEntryResponseMessageBuilder : D99BMessageBuilder<ICCSEntryResponseTypeX, CUSRESMessage, EDIMessage>
	{
		public CCSEntryResponseMessageBuilder(ICCSEntryResponseTypeX data, MessageSubTypes messageSubType)
			: base(data, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage()
		{
			CreateUNH();
			CreateBGM();
			CreateDTM();
			CreateGroup3();
			CreateGroup4();
			CreateGroup6();
		}

		void CreateUNH()
		{
			var unh = edifactMessage.UNH.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateUNH(unh, EDIMessage.MessageNumberPlaceHolder, MessageTypeList.CustomsResponseMessage, "S", "99B", ControllingAgencyList.UnCefact);
		}

		void CreateBGM()
		{
			var bgm = edifactMessage.BGM.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateBGM(bgm, data.BatchNumber, ZString.Empty, MessageFunctionCodeList.Original);
		}

		void CreateDTM()
		{
			var dtm = edifactMessage.DTM.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateDTM(dtm, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime, ZDateTime.Now, "yyyyMMdd", DateTimePeriodFormatCodeList.Ccyymmdd);
		}

		void CreateGroup3()
		{
			var group3 = edifactMessage.Group3.InstantiateAChildAndAddItToChildrenCollection();
			var rff = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.DeclarantsCustomsIdentityNumber, data.AccountSecurityNumber);
		}

		void CreateGroup4()
		{
			var group4 = edifactMessage.Group4.InstantiateAChildAndAddItToChildrenCollection();

			foreach (var responseCode in data.ResponseCodes)
			{
				var erp = group4.ERP.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateERP(erp, null, responseCode.MessageItemNumber, ZString.Empty);

				var rff = group4.RFF.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateRFF(rff, ReferenceFunctionCodeQualifierList.OriginatorsReference, responseCode.ApplicableReferenceNumber);

				var erc = group4.ERC.InstantiateAChildAndAddItToChildrenCollection();
				D99BMessageUtilities.PopulateERC(erc, responseCode.CSSErrorMessageNumber);
			}
		}

		void CreateGroup6()
		{
			var group6 = edifactMessage.Group6.InstantiateAChildAndAddItToChildrenCollection();
			var doc = group6.DOC.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateDOC(doc, DocumentNameCodeList.GeneralResponseCustoms, ZString.Empty);

			CreateGroup11(group6);
		}

		void CreateGroup11(SegmentGroup6 group6)
		{
			var group11 = group6.Group11.InstantiateAChildAndAddItToChildrenCollection();
			var cst = group11.CST.InstantiateAChildAndAddItToChildrenCollection();
			D99BMessageUtilities.PopulateCST(cst, ZString.Empty, data.TotalNumberOfEntries.ToString(), data.TotalNumberOfValidEntries.ToString(), data.TotalNumberOfInvalidEntries.ToString(), ZString.Empty, ZString.Empty);
		}
	}
}
