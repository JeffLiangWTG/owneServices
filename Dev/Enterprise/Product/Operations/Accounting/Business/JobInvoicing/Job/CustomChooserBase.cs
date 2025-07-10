using System;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Dynamic;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class CustomChooserBase
	{
		protected CustomChooserBase(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		protected BusinessObjectFactory Factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling any code entered as configuration")]
		public ZString GetCodeWithConfiguration(IJobInvoicingPlugIn plugin, string customConfig = null)
		{
			ZString code = ZString.Empty;
			if (string.IsNullOrEmpty(customConfig))
			{
				customConfig = GetCustomDefaultConfiguration();
			}

			var parentBusinessObject = plugin as BusinessObject;
			if (parentBusinessObject == null)
			{
				var businessObjectProvider = plugin as IBusinesssObjectProviderForDocumentWrapper;
				parentBusinessObject = businessObjectProvider?.BusinessObjectForDocumentWrapper;
			}

			if (parentBusinessObject != null && !string.IsNullOrEmpty(customConfig))
			{
				var customConfigNoRtf = ORtfTextUtil.RtfToText(customConfig);
				var wrapperCreator = ObjectFactory.Get<IDocFreightWrapperCreator>();
				var wrapper = wrapperCreator.CreateFreightWrapper(parentBusinessObject, parentBusinessObject.Factory);
				try
				{
					code = GetBizCode(customConfigNoRtf, wrapper);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}
			}

			return code;
		}

		ZString GetBizCode(ZString customConfig, IDocumentWrapper wrapper)
		{
			DefineMethodAndRunScript(customConfig);
			var lambda = CreateLambda();
			return lambda(wrapper, GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);
		}

		protected Tuple<IJobInvoicingPlugIn, ZString> GetConsumerOfJob(ZGuid jobGuid)
		{
			IJobInvoicingPlugIn objToValidate = null;
			ZString message = ZString.Empty;
			if (jobGuid.IsEmpty)
			{
				message = Res.GetString("C23577EF-B8F9-4DC1-B37D-AA5CA033B3B9", "Please select a Job to validate.");
				return new Tuple<IJobInvoicingPlugIn, ZString>(objToValidate, message);
			}

			var job = Factory.Load<Job>(jobGuid);
			var genericJob = job?.LoadGenericJob();
			objToValidate = genericJob?.Consumer;

			if (objToValidate == null)
			{
				message = Res.GetString("AA559816-B12A-4657-BE90-4EDF872ACBC5", "Error validating with selected Job, please select a different one.");
			}

			return new Tuple<IJobInvoicingPlugIn, ZString>(objToValidate, message);
		}

		Func<object, GlbCompany, GlbBranch, GlbDepartment, string> CreateLambda()
		{
			return Proxy.CreateLambda<Func<object, GlbCompany, GlbBranch, GlbDepartment, string>>(GetDefaultMethodForLambda(), (NoResString)"obj", (NoResString)"company", (NoResString)"branch", (NoResString)"department");
		}

		void DefineMethodAndRunScript(ZString customConfig)
		{
			var sb = new StringBuilder();
			sb.Append(GetDefiningMethod());
			sb.AppendLine();
			var lines = customConfig.Split('\n');
			foreach (var line in lines)
			{
				sb.Append('\t' + line);
			}
			Proxy.RunScript(sb.ToString());
		}

		protected abstract string GetCustomDefaultConfiguration();

		protected abstract string GetDefiningMethod();

		protected abstract string GetDefaultMethodForLambda();

		IDlrProxy Proxy
		{
			get
			{
				if (proxy == null)
				{
					proxy = new DlrProxy();
				}
				return proxy;
			}
		}
		IDlrProxy proxy;
	}
}
