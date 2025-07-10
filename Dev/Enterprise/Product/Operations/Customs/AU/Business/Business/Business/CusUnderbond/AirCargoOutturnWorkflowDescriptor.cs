using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AirCargoOutturnWorkflowDescriptor : Customs.Business.AirCargoWorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.CusUnderbond.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.CusUnderbond.MultilingualDescription; }
		}

		#endregion

		public override Type WorkflowProviderType
		{
			get { return typeof(CusUnderbond); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.CFSAirCargoOutturn }; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy;
		}

		public override bool SupportsEventTracking => true;

		/// <summary>
		/// Provides action types that are specific to AirCargoOutturnWorkflowDescriptor and available for any ProcessTask
		/// </summary>
		/// <param name="task">The ProcessTask is ignored</param>
		/// <returns>The empty list</returns>
		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			return new CodeDescriptionPairList();
		}

		public override bool SupportsBufferManagement => true;

		public override ControllerID ControllerID => ControllerIDs.Customs.AU.AirCargoOutturnBillsController;
	}
}
