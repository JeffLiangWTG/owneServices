using System.Collections.Generic;
using System.Drawing;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	[System.Diagnostics.DebuggerDisplay("{Entity.Name}, X:{X}, Y:{Y}")]
	public class AnnotationViewModel : NodeViewModel
	{
		public AnnotationViewModel(INetworkEntity entity, NetworkViewModel networkViewModel)
			: base(entity, networkViewModel)
		{
		}

		protected override IEnumerable<ColorOffset> GetStatusColors()
		{
			return new List<ColorOffset>() { new ColorOffset(Color.LightBlue, 0) };
		}

		public override string CompletionCriteriaPlaceholder
		{
			get { return string.Empty; }
		}

		public override bool IsCompletionCriteriaVisible
		{
			get { return false; }
		}

		public override bool IsEntityDecorationVisible
		{
			get { return false; }
		}

		public override bool SupportsChildEntities
		{
			get { return false; }
		}

		public override bool SupportsEditEntity
		{
			get { return false; }
		}

		public override bool SupportsShowItems
		{
			get { return false; }
		}

		public override bool IsEditableNotesVisible
		{
			get { return true; }
		}

		public override bool ShowScheduleDetails
		{
			get { return false; }
		}
	}
}
