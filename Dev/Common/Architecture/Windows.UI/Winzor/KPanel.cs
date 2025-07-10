namespace CargoWise.Windows.UI
{
	public partial class KPanel
	{
		public new bool Draggable { get => base.Draggable; set => base.Draggable = value; }

		public KBorderStyle htmlBorder;

		protected override string ControlStyleString => $"{base.ControlStyleString}{htmlBorder.GetBorderStyleString()}";
	}
}
