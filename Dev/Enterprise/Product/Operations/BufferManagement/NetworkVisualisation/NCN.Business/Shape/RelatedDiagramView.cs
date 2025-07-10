using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class RelatedDiagramView : NonPersistentBusinessObject
	{
		public RelatedDiagramView(BMNCNShape mainDiagram, BMNCNShape shape)
			: base(shape.Factory)
		{
			this.shape = shape;
			this.mainDiagram = mainDiagram;
		}
		readonly BMNCNShape shape;
		readonly BMNCNShape mainDiagram;

		public BMNCNShape Shape
		{
			get { return shape; }
		}

		public ZString Name
		{
			get { return shape.BNS_Name; }
		}

		public ZString RelationshipType
		{
			get { return GetCalculatedRelationshipType(); }
		}

		ZString GetCalculatedRelationshipType()
		{
			if (shape.BNS_RelatedEntityID == mainDiagram.PK)
			{
				return LinkedRelation;
			}

			var result = ParentRelation;

			if (mainDiagram.AllAttachments.FirstOrDefault(x => x.BNA_Type == AttachmentTypeList.Codes.SwitchToScaled &&
					x.BNA_BNS_FromShape == shape.PK) != null)
			{
				result = NonScaledSource;
			}

			if (mainDiagram.AllAttachments.FirstOrDefault(x => x.BNA_Type == AttachmentTypeList.Codes.SwitchToScaled &&
					x.BNA_BNS_ToShape == shape.PK) != null)
			{
				result = ScaledCopy;
			}

			return result;
		}

		public static string ParentRelation
		{
			get { return Res.GetString("ABCCD94E-E113-4BF1-AA7E-0456BE378620", "Parent diagram"); }
		}

		public static string LinkedRelation
		{
			get { return Res.GetString("732193A7-C42C-4A18-807D-E91AA049488F", "Linked diagram"); }
		}

		public static string NonScaledSource
		{
			get { return Res.GetString("AF9BE807-0A1E-47E5-9B0C-70A43F0BA1D0", "Non-scaled source"); }
		}

		public static string ScaledCopy
		{
			get { return Res.GetString("6BB1B6AD-447C-4DAC-BC14-61C401CCDE9C", "Scaled copy"); }
		}

		public ZBool IsScaled
		{
			get { return shape.IsScaled; }
		}

		public bool IsApproved
		{
			get { return shape.IsApproved; }
		}

		public ZDateTime ScheduledStartTimeLocal
		{
			get { return shape.ScheduledStartTimeLocal; }
		}

		public ZDateTime ScheduledFinishTimeLocal
		{
			get { return shape.ScheduledFinishTimeLocal; }
		}
	}
}
