using System.Linq;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class JobNetworkValidator : IJobNetworkValidator
	{
		public void Validate(IJobNetwork network)
		{
			ValidateNetworkEntities(network);
			ValidateShapesExcludingLinkedProcessHeaders(network);
		}

		void ValidateNetworkEntities(IJobNetwork network)
		{
			network.DiagramEntity.RunPreSaveValidation();
		}

		void ValidateShapesExcludingLinkedProcessHeaders(IJobNetwork network)
		{
			foreach (var shape in network.Shapes)
			{
				shape.Validation.ValidateAll();
			}
		}

		public void ValidateLoops(IJobNetwork network, string progressReporterCaption)
		{
			progressReporterCaption = progressReporterCaption ?? Res.GetString("C217E885-7B8F-4117-9795-6445B9F8FC74", "Validating diagram");

			using (var progressReporter = network.Controller.UserInteractionImplementor.ProgressReporterProvider.CreateProgressReporter(progressReporterCaption, numberOfItemsToProcess: network.Entities.ShapeEntities.Count()))
			{
				foreach (var entity in network.Entities.ShapeEntities)
				{
					if (progressReporter.IsCancelled)
					{
						return;
					}

					entity.Validation.ValidateProcessHeaderLoops();

					progressReporter.ReportOneItemProcessed();
				}
			}
		}
	}
}
