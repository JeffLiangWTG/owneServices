using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICPQAAttachee : IBusiness
	{
		ZGuid PK { get; }
		SchemaGuidColumn FKColumnInCusEntryCPDecTable { get; }
		CMRCusEntryCPDecCollection Questions { get; }
		ZDateTime SelectionDate { get; }
		void RegisterEditableChildObject(IBusiness child);
	}

	public struct LodgementQuestionKeys
	{
		public ZBool IsSAC;
		public ZBool IsSACWithLine;
		public ZBool IsPaidUnderProtest;
		public ZBool IsABNQuotedForLCTAndWET;
		public ZBool IsNature30;
		public ZBool IsNature20;
		public ZDecimal TotalCustomsValue;
		public ZBool IsSea;
		public ZBool HasFCLOrFCXLines;
		public ZBool HasLCLLines;
		public ZBool IsPaid;
		public ZBool IsGSTDeferred;
		public ZBool IsRefundAmendment;
		public ZBool HasSecurityTreatment;
		public ZBool IsUPEDeclaration;
		public ZBool IsSOFADeclaration;
		public ZBool HasRemissionOnBunkerFuels;
	}

	public interface ICPQAHeaderAttachee : ICPQAAttachee
	{
		LodgementQuestionKeys LodgementQuestionKey { get; }
		bool IsStatusPostLodge { get; }
	}

	public struct CPQuestionKeys
	{
		public ZString TariffNumber;
		public ZString StatCode;
		public ZString OriginCode;
		public ZString Nature;
		public ZString ModeOfTransport;
		public bool HasValidOriginOrNatureOrModeOfTransport;
	}

	public interface ICPQALineAttachee : ICPQAAttachee
	{
		CPQuestionKeys CPQuestionKey { get; }
		ICPQALineAttachee[] SourcesToDefault { get; }
		LineDefaultQuestions DefaultUniqueQuestions { get; }
		ZString TableCode { get; }
		ZBool IsRiskCalculatedFromTariff { get; }
		ZBool IsRiskHistorySupported { get; }
	}

	public interface ICPQAAttacheeHolder
	{
		ZGuid PK { get; }
		ICPQAHeaderAttachee[] Headers { get; }
		ICPQALineAttachee[] Lines { get; }
		CachedAnsweredQuestions CachedQuestions { get; }
	}

	public interface ISelfHoldingLineAttachee : ICPQAAttacheeHolder, ICPQALineAttachee
	{
	}
}
