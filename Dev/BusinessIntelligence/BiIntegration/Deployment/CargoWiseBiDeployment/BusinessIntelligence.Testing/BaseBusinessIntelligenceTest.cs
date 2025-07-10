using System;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Bi.BusinessIntelligence.Testing
{
	[RequiresSoftware(RequiredSoftware.IsVM | RequiredSoftware.SsasTabular2016OrLater | RequiredSoftware.OlapServer)]
	[DatCapabilityRequirement("SOURCE_CODE")]
	public abstract class BaseBusinessIntelligenceTest : TestCase
	{
		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			if (!string.IsNullOrEmpty(CubeName))
			{
				SsasHelper.RestoreModelBackup(SsasServerConnection, CubeName);
			}
		}

		protected override void FinalTearDown()
		{
			if (!string.IsNullOrEmpty(CubeName))
			{
				SsasHelper.DropModel(SsasServerConnection, CubeName);
			}
			base.FinalTearDown();
			Release(ref ssasServerConnection);
		}

		static void Release<T>(ref T disposable)
			where T : class, IDisposable
		{
			if (disposable != null)
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("Error caught on object dispose.", ex);
				}

				disposable = null;
			}
		}

		protected virtual string CubeName { get; }

		#region Connections

		protected SsasServer SsasServerConnection
		{
			get
			{
				return ssasServerConnection ?? (ssasServerConnection = SsasServer.New(SsasHelper.AnalysisServerName));
			}
		}
		SsasServer ssasServerConnection;

		#endregion
	}
}
