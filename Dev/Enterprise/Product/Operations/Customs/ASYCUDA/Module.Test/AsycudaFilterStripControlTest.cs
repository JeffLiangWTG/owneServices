using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	sealed class AsycudaFilterStripControlTest : TestCaseWithFactory
	{
		public void TestGridColums()
		{
			IReadOnlyList<string> expectedColumnNamesInSortOrderList =
				[
					nameof(AsycudaManifestHeader.ManifestApplicationType),
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
					AsycudaManifestHeader.Schema.AMA_GS_NKCustomsAgent,
					AsycudaManifestHeader.Schema.SpecificCircumstanceIndicator,
				];

			using (var form = new ZForm())
			using (var filterStrip = new AsycudaFilterStripControl(new AsycudaManifestModuleCollection(Factory), new AsycudaFilterStrip()))
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(filterStrip);
					form.Show();
					var grid = filterStrip.FilteredGrid;
					var columnNamesInSortOrder = expectedColumnNamesInSortOrderList;
					for (var i = 0; i < columnNamesInSortOrder.Count; i++)
					{
						AssertEquals(i.ToString(), columnNamesInSortOrder[i], grid.Columns[i].ColumnName);
					}
				});
			}
		}
	}
}
