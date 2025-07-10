using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class DiagramBoardSectionConfiguration : NonPersistentBusinessObject<DiagramBoardSectionConfigurationValidation>, IBoardSectionConfigurationBizo
	{
		public DiagramBoardSectionConfiguration(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Xml Properties

		#region DiagramPK

		[XmlColumnProperty]
		[List("Diagrams")]
		[ResourceStringData("DiagramBoardSectionConfiguration.DiagramPK", Caption = "Diagram", FullDescription = "The diagram to show on this board section")]
		public ZGuid DiagramPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(DiagramPKInfo); }
			set { SetXmlColumnPropertyValue(DiagramPKInfo, value); }
		}

		public ZPropertyInfo DiagramPKInfo
		{
			get { return GetZPropertyInfo(nameof(DiagramPK)); }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public BMNCNShape Diagram
		{
			get { return Factory.Load<BMNCNShape>(DiagramPK); }
		}

		public DiagramShapeCollection Diagrams
		{
			get { return Factory.GetCachedValue("DiagramShapeCollection", () => new DiagramShapeCollection(Factory)); }
		}

		#endregion

		#region IBoardSectionConfigurationBizo Members

		public ZString SectionName
		{
			get
			{
				var diagram = Diagram;
				return diagram != null ? diagram.Name : string.Empty;
			}
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(SectionName)); }
		}

		public void CopyConfigurationPropertiesToNewSection(IBMBoardSection section)
		{
		}

		#endregion

		#region Overrides

		public override DiagramBoardSectionConfigurationValidation GetNewValidation()
		{
			return new DiagramBoardSectionConfigurationValidation(this);
		}

		#endregion
	}
}
