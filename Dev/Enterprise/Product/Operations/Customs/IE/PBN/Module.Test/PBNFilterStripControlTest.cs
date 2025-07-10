using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.Customs.IE.PBN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Module.Test
{
	[TestedType(typeof(PBNFilterStripControl))]
	sealed class PBNFilterStripControlTest : TestCaseWithFactory
	{
		public void TestColumnsExist()
		{
			using (var userControl = new AsycudaPreBoardingNotificationFilterStripControl())
			{
				var grid = userControl.Grid;
				var columnNamesInSortOrder = ExpectedColumnNamesInSortOrderList;
				for (var i = 0; i < columnNamesInSortOrder.Count; i++)
				{
					AssertNotNull(grid.GetColumnStyle(columnNamesInSortOrder[i]));
				}
			}
		}

		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new List<ZString>
					{
						nameof(AsycudaManifestHeader.ManifestApplicationType),
						nameof(AsycudaManifestHeader.RegistrationNumber),
						AsycudaManifestHeader.Schema.AMA_JobReference,
						AsycudaManifestHeader.Schema.AMA_TransportMode,
						AsycudaManifestHeader.Schema.AMA_RL_NKPortOfDischarge,
						AsycudaManifestHeader.Schema.AMA_E_ARV,
						AsycudaManifestHeader.Schema.AMA_RN_NKConveyanceNationality,
						AsycudaManifestHeader.Schema.AMA_VesselName,
						AsycudaManifestHeader.Schema.AMA_Voyage,
						AsycudaManifestHeader.Schema.AMA_RN_NKCountry,
						nameof(AsycudaManifestHeader.MessageStatus),
						AsycudaManifestHeader.Schema.AMA_Nature,
						nameof(AsycudaManifestHeader.RegistrationStatus),
						AsycudaManifestHeader.Schema.AMA_E_DEP,
						AsycudaManifestHeader.Schema.AMA_RL_NKPortOfLoading,
						AsycudaManifestHeader.Schema.AMA_AgentType,
						AsycudaManifestHeader.Schema.AMA_MasterBill,
						nameof(AsycudaManifestHeader.RegistrationDate),
						AsycudaManifestHeader.Schema.AMA_GS_NKCustomsAgent
					};
				}

				return expectedColumnNamesInSortOrderList;
			}
		}
		List<ZString> expectedColumnNamesInSortOrderList;
	}
}
