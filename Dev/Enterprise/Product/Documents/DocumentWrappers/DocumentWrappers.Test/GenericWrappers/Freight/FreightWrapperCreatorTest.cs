using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class FreightWrapperCreatorTest : TestCaseWithFactory
	{
		public void TestGetFreightWrapperType_CheckWrapperTypeForAllJobTypes()
		{
			var jobTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
			IDocFreightWrapperCreator wrapperCreator = new FreightWrapperCreator();
			foreach (JobInvoicingConsumerType jobType in jobTypes)
			{
				Type wrapperType = null;
				AssertNoExceptionThrown(string.Format("Business Object Type : {0}", jobType.BizoType.ToString()), () => { wrapperType = wrapperCreator.GetFreightWrapperType(jobType.BizoType); });
				var jobCode = jobType.Code;
				if (jobCode == "CTO" || jobCode == "AHE" || jobCode == "MAN"
					|| jobCode == "CAE" || jobCode == "WSJ" || jobCode == "WVO"
					|| jobCode == "ABK" || jobCode == "LPC" || jobCode == "STO")
				{
					// BizO doesn't support documents, so we expect there to be no wrapper. 
					AssertNull("Wrapper for job type " + jobCode + " was unexpectedly not null. Did you recently add support for documents to this bizO?  If so, remove it from the list of exclusions.", wrapperType);
				}
				else
				{
					AssertNotNull("Wrapper for job type " + jobCode + " was unexpectedly null.  Most likely, the bizO does not support documents. If that's the case, add it to the exclusions above. When you're ready to add support for documents, probably via NewFreightWrapper, remove it from the exclusions.  If you're reading this and you appreciate the detail, the lesson to learn is that detailed test failure messages are good, so tell your friends", wrapperType);
				}
			}
		}
	}
}
