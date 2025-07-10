using System.Collections.Generic;
using Enterprise.Client.EDI.Business.Test;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(FreightWrapperFromSupportIncident))]
	internal sealed class FreightWrapperFromSupportIncidentTest : FreightWrapperEDITest<SupportIncident>
	{
		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string> { { "JobNumber", "CJ4RDL5ZJS6QCQ4MMXR1" }, { "JobNumberHeading", "Job Number" }, { "JobNumberBarcodeText", "^IRQ=CJ4RDL5ZJS6QCQ4MMXR1;;|" }, { "JobNumberBarcodeTextForFont", "È^IRQ=CJ4RDL5ZJS6QCQ4MMXR1;;|^Ê" }, { "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈCJ4RDL5ZJS6QCQ4MMXR1BÊ" }, };
			}
		}

		protected override GenericWrapper GetNewFreightWrapperCore()
		{
			return new FreightWrapperFromSupportIncident(ediBusinessObject, Factory);
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^" + IncidentConstants.SupportIncidentDocManagerCode + "=T8FJWPPHFFGYN7GZ6CQO;CAD;|IÊ";
		}
	}
}
