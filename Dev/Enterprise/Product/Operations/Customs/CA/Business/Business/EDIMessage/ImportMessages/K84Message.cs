using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class K84Message : EDIMessage, IDocumentSupportable, IControllerIDProvider
	{
		public K84Message(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", ApplicationCodes.CAIMP).GetNextFormatted(Factory);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.CAIMP;
			EM_MessageType = MessageTypeList.Codes.K84Report;
		}

		public override ZDateTime K84AccountingDate
		{
			get
			{
				if (EM_MessageSubType == K84ReportTypes.Codes.Daily)
				{
					return this.GetSystemDefinedValue<ZDateTime>(Schema.K84AccountingDate);
				}
				return base.K84AccountingDate;
			}
		}

		public override ZDateTime K84StatementDate
		{
			get
			{
				if (EM_MessageSubType == K84ReportTypes.Codes.Daily || EM_MessageSubType == K84ReportTypes.Codes.Monthly)
				{
					return this.GetSystemDefinedValue<ZDateTime>(Schema.K84StatementDate);
				}
				return base.K84StatementDate;
			}
		}

		#region Properties

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new K84ReportTypes(); }
		}

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return DocumentSupporter; }
		}

		K84DocumentSupporter DocumentSupporter
		{
			get { return new K84DocumentSupporter(this); }
		}

		//public virtual bool RequiresDocumentUDFPlugIn
		//{
		//  get { return false; }
		//}

		#endregion

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.CA.K84Reports; }
		}

		#endregion
	}
}
