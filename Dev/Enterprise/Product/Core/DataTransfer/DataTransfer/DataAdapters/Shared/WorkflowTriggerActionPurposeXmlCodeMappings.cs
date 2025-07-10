using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business.WorkflowManager;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class WorkflowTriggerActionPurposeXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		WorkflowTriggerActionPurposeXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(ProcessTaskTriggerPurposeList.Codes.AsPerPayload, nameof(Xsd.WorkflowTriggerActionPurpose.APP));
			yield return new Mapping(ProcessTaskTriggerPurposeList.Codes.Event, nameof(Xsd.WorkflowTriggerActionPurpose.EVT));
			yield return new Mapping(ProcessTaskTriggerPurposeList.Codes.Invoice, nameof(Xsd.WorkflowTriggerActionPurpose.INV));
		}

		public static readonly WorkflowTriggerActionPurposeXmlCodeMappings Instance = new WorkflowTriggerActionPurposeXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Workflow Trigger Action Purpose"; }
		}

		public new Xsd.WorkflowTriggerActionPurpose GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.WorkflowTriggerActionPurpose.APP, errorContext, notify);
		}
	}
}
