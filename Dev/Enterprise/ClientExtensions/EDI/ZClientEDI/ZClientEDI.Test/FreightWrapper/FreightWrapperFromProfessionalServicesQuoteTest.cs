using System.Collections.Generic;
using Enterprise.Client.EDI.Business.Test;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(FreightWrapperFromProfessionalServicesQuote))]
	internal sealed class FreightWrapperFromProfessionalServicesQuoteTest : FreightWrapperEDITest<ProfessionalServicesQuote>
	{
		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string> { { "JobNumber", "CJ4RDL5ZJS6QCQ4MMX" }, { "JobNumberHeading", "Job Number" }, { "JobNumberBarcodeText", "^INC=CJ4RDL5ZJS6QCQ4MMX;;|" }, { "JobNumberBarcodeTextForFont", "È^INC=CJ4RDL5ZJS6QCQ4MMX;;|BÊ" }, { "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈCJ4RDL5ZJS6QCQ4MMXsÊ" }, };
			}
		}

		protected override GenericWrapper GetNewFreightWrapperCore()
		{
			return new FreightWrapperFromProfessionalServicesQuote(ediBusinessObject, Factory);
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^INC=T8FJWPPHFFGYN7GZ6C;CAD;|&Ê";
		}
	}
}
