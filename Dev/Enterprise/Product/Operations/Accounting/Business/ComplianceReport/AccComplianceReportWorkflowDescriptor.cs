using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.AccComplianceReportCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("fa1b9e36-04f1-4d9f-b2d9-21b6548eef09", "Accounting Compliance Report"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(AccComplianceReport); }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo)
		{
			return false;
		}

		public override bool SupportsOtherCompanyAPInvoiceImport
		{
			get { return false; }
		}

		public override bool SupportsWorkflowTriggerActionUniversalTransactionXML
		{
			get { return false; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.ComplianceReport }; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AccComplianceReport; }
		}

		public override bool SupportsWorkflowTriggerActionUniversalTransactionBatchXML
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy;
		}

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("dfb7b13a-0adc-42e2-a86d-3ce3322b1f69", "Report Type"), ReportTypeList));
				return result.ToArray();
			}
		}

		CodeDescriptionPairList ReportTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Cast<ComplianceReportConfiguration>().
					Select(x => new CodeDescriptionPair(x.ReportCode.ToString(), x.ReportTitle.ToString())).ToList());

				return result;
			}
		}
	}
}
