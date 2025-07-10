using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class AcceptabilityBandMatchesVisibilityFilter : CardVisibilityFilter, IStatefulBoardFilter
	{
		public AcceptabilityBandMatchesVisibilityFilter(BMComponentAcceptabilityBand band, ZGuid boardSectionAcceptabilityBandPK, AcceptabilityBandSqlBuilderParameters parameters)
		{
			BoardSectionAcceptabilityBandPK = boardSectionAcceptabilityBandPK;
			this.band = band;
			this.parameters = parameters;

			if (band.UsesFilterStrips)
			{
				this.workflowPKs = WorkflowFactory.GetMatchingWorkflows(band, parameters);
			}
		}

		public ZGuid BoardSectionAcceptabilityBandPK { get; }
		readonly BMComponentAcceptabilityBand band;
		Task<Dictionary<ZGuid, IWorkflow>> workflowPKs;
		readonly AcceptabilityBandSqlBuilderParameters parameters;

		public bool DidGetCellVisibilityApplicatorCauseErrorOnBackgroundThread { get; private set; }

		public override bool AllowMultiple
		{
			get { return false; }
		}

		public override string FilterName
		{
			get { return Res.GetString("9792787c-a444-4d92-9dbf-1cb57ceb2958", "Showing items matching Acceptability Band {0}.", band.BAB_Name); }
		}

		public override void FetchForFilter(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			// No fetch required.
		}

		public override CellVisibilityApplicator GetCellVisibilityApplicator(BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			return (cardContent, cell) =>
			{
				if (workflowPKs != null)
				{
					if (cardContent == null)
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "cardContent is null. cell channel full name: {0}, cell coordinates X {1} and Y {2}, cell.Label: {3}",
							cell?.Channel?.GetChannelName(DisplayNameType.FullName), cell?.Coordinates.X, cell?.Coordinates.Y, cell?.Label));
						return true;
					}

					var result = workflowPKs.Result;

					if (result == null)
					{
						DidGetCellVisibilityApplicatorCauseErrorOnBackgroundThread = true;
						return false;
					}

					return result.ContainsKey(cardContent.WorkflowIdentifier);
				}

				return true;
			};
		}

		public override bool Equals(object obj)
		{
			var otherFilter = obj as AcceptabilityBandMatchesVisibilityFilter;
			return otherFilter != null && otherFilter.band.PK == band.PK;
		}

		public override int GetHashCode()
		{
			return GetType().Name.GetHashCode();
		}

		void IStatefulBoardFilter.Refresh()
		{
			if (workflowPKs != null)
			{
				workflowPKs = WorkflowFactory.GetMatchingWorkflows(band, parameters);
			}
		}
	}
}
