using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusExitDetail : AutoCusExitDetail, Integration.Customs.ES.ICusExitDetail, IESMessageInfoProvider, IESResponseBusinessObject
	{
		public CusExitDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		#region Schema
		public new partial class Schema : AutoCusExitDetail.Schema
		{
			public const string FormattedCircuit = "FormattedCircuit";
		}
		#endregion

		[ReadOnly(true)]
		public override ZDateTime ZG_AcceptanceDate { get => base.ZG_AcceptanceDate; set => base.ZG_AcceptanceDate = value; }

		[ReadOnly(true)]
		public override ZString ZG_CSVClearance { get => base.ZG_CSVClearance; set => base.ZG_CSVClearance = value; }

		[List(nameof(Lookups) + "." + nameof(CusExitDetailLookups.ArrivalNotificationCodeList))]
		public override ZString CED_ArrivalNotificationPlace { get => base.CED_ArrivalNotificationPlace; set => base.CED_ArrivalNotificationPlace = value; }

		public override ZString CED_Status
		{
			get => base.CED_Status;
			set
			{
				var oldValue = CED_Status;
				if (oldValue != value)
				{
					base.CED_Status = value;
					SetBOReadOnly();
				}
			}
		}
		public override void OnLoaded()
		{
			base.OnLoaded();
			if (!ReadOnly)
			{
				SetBOReadOnly();
			}
		}

		#region New Properties

		public ZString FormattedCircuit
		{
			get
			{
				var code = ZG_Circuit;
				var description = (ZString)AddInfoLookups.CircuitCodeList.GetDescriptionFromCode(code);

				return !description.IsEmpty ? description : code;
			}
		}
		public ZPropertyInfo FormattedCircuitInfo => ZG_CircuitInfo;
		#endregion

		public AddInfoCusExitDetailLookups AddInfoLookups => new AddInfoCusExitDetailLookups(AddInfo);

		GlbStaff IESMessageInfoProvider.Broker => Header?.CustomsAgent;

		ZString IESMessageBusinessObject.EntryReference => CED_MovementReferenceNumber;

		ZString IESMessageInfoProvider.MRN => CED_MovementReferenceNumber;
		ZString IESMessageInfoProvider.DocumentJobReference => CED_MovementReferenceNumber;

		ZGuid IESResponseBusinessObject.BranchPK => (Header?.CEH_Parent as JobDeclaration)?.Branch.PK ?? ZGuid.Empty;

		EDIMessageCollection IESResponseBusinessObject.MessageCollection => Messages;

		void SetBOReadOnly() => SetReadOnlyIncludingChildren(NeedsLockForEdit(CED_Status));

		bool NeedsLockForEdit(ZString status) => status == MessageStatusList.Codes.AwaitingResponse || status == EntryStatusCodes.Cleared || status == EntryStatusCodes.CustomsDeclarationAccepted;

		#region TypeSafe

		public new CusExitDetailValidation Validation => (CusExitDetailValidation)base.Validation;
		protected override EU.Business.CusExitDetailValidation GetNewValidation() => new CusExitDetailValidation(this);

		public new CusExitDetailLookups Lookups => (CusExitDetailLookups)base.Lookups;

		protected override EU.Business.CusExitDetailLookups GetNewLookups() => new CusExitDetailLookups(this);

		#endregion

		public void UpdateCSVClearance(ZString csvCodeFromUser)
		{
			if (CSVCodeIsValid(csvCodeFromUser))
			{
				var oldValue = ZG_CSVClearance;
				ZG_CSVClearance = csvCodeFromUser;

				var logTypeCode = "CSV";
				var logReason = (NoResString)"Manually Added CSV Clearance Code to Exit Movement " + CED_MovementReferenceNumber;
				var parameters = new KeyValuePair<string, string>[]
				{
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Old, oldValue),
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.New, ZG_CSVClearanceInfo.Value.ToString()),
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, logTypeCode),
							new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, logReason)
				};

				Logs.AddNew(ZArchitecture.Business.Events.ChangeOfIdentifier, parameters);
			}
		}

		bool CSVCodeIsValid(ZString code) => Regex.IsMatch(code, @"^[a-zA-Z0-9]+$");

		public bool IsSentOrAccepted => CED_Status == MessageStatusList.Codes.AwaitingResponse || CED_Status == EntryStatusCodes.Cleared || CED_Status == EntryStatusCodes.CustomsDeclarationAccepted;
	}
}
