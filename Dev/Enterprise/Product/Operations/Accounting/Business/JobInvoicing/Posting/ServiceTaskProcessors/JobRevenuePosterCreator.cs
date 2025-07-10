using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	class JobRevenuePosterCreator : IRevenuePosterCreator
	{
		public IProcessor CreateRevenuePoster(IWorkflowProvider provider)
		{
			IProcessor result = null;

			var plugIn = provider as IJobInvoicingPlugIn;
			if (plugIn != null && !(plugIn is Enterprise.Integration.Forwarding.IForwardingConsol))
			{
				result = new JobRevenuePoster(plugIn);
			}

			return result;
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	using System;
	using Enterprise.BufferManagement.Integration;
	using Enterprise.ZArchitecture.Business;

	class DummyIWorkflowProvider : IWorkflowProvider
	{
		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			throw new NotImplementedException();
		}

		public IProcessHeaderCollection Workflows => throw new NotSupportedException();

		public ProcessTaskCollection WorkflowItems
		{
			get { throw new NotImplementedException(); }
		}

		public CargoWise.Integration.IColumnValueRanker GetTemplateSelectionCriteria()
		{
			throw new NotImplementedException();
		}

		public CargoWise.Types.ZGuid PK
		{
			get { throw new NotImplementedException(); }
		}

		public CargoWise.Types.ZString WorkflowType
		{
			get { throw new NotImplementedException(); }
		}

		public CargoWise.Types.ZGuid Identifier
		{
			get { throw new NotImplementedException(); }
		}

		public Logs Logs => throw new NotImplementedException();

		public BusinessObjectFactory LogsFactory => throw new NotImplementedException();

		public void FillWithValidDataForWorkflowFilterTests(string moduleId)
		{
		}
	}
}

#endif
#endregion
