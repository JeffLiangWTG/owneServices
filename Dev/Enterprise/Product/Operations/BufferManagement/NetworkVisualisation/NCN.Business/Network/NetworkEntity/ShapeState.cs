using System;
using System.Collections.Generic;
using System.Drawing;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Network.NetworkEntity
{
	[Serializable]
	public class ShapeState
	{
		public ShapeState() => Children = new List<ShapeState>();
		public List<ShapeState> Children { get; set; }
		public string PK { get; set; }
		public string Name { get; set; }
		public string JobName { get; set; }
		public Color BackColor { get; set; }
		public string CompletionCriteria { get; set; }
		public string ShapeNotes { get; set; }
		public double Width { get; set; }
		public Color ForeColor { get; set; }
		public double Height { get; set; }
		public string Style { get; set; }
		public string JobType { get; set; }
		public string LayoutData { get; set; }
		public string RelatedEntityID { get; set; }
		public string RelatedEntityTableCode { get; set; }
		public string ShapeType { get; set; }
		public string Status { get; set; }
		public string AdditionalDetails { get; set; }
		public string BNS_RelatedEntityTableCode { get; set; }
		public string BNS_RelatedEntityID { get; set; }
	}
}
