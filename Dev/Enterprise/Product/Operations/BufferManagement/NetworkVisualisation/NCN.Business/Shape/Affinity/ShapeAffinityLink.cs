using System.ComponentModel;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class ShapeAffinityLink : NonPersistentBusinessObject
	{
		public ShapeAffinityLink(BusinessObjectFactory factory)
				: base(factory)
		{
		}

#if DEBUG
		public ShapeAffinityLink(ZGuid shape, ZGuid shapeAffinity)
		{
			ShapePK = shape;
			ShapeAffinityPK = shapeAffinity;
		}
#endif

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			IsApplied = ZBool.True;
			TimeApplied = ZDateTime.UtcNow;
		}

		#endregion

		#region Properties

		#region ShapePK

		[XmlColumnProperty]
		public ZGuid ShapePK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(ShapePKInfo); }
			set { SetXmlColumnPropertyValue(ShapePKInfo, value); }
		}

		public ZPropertyInfo ShapePKInfo
		{
			get { return GetZPropertyInfo(nameof(ShapePK)); }
		}

		#endregion

		#region ShapeAffinityPK

		[XmlColumnProperty]
		public ZGuid ShapeAffinityPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(ShapeAffinityPKInfo); }
			set { SetXmlColumnPropertyValue(ShapeAffinityPKInfo, value); }
		}

		public ZPropertyInfo ShapeAffinityPKInfo
		{
			get { return GetZPropertyInfo(nameof(ShapeAffinityPK)); }
		}

		#endregion

		#region IsApplied

		[ReadOnly(true)]
		[XmlColumnProperty(SerialiseDefaultValues = true)]
		public ZBool IsApplied
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsAppliedInfo); }
			set
			{
				if (IsApplied && !value)
				{
					TimeRemoved = ZDateTime.UtcNow;
				}

				SetXmlColumnPropertyValue(IsAppliedInfo, value);
			}
		}

		public ZPropertyInfo IsAppliedInfo
		{
			get { return GetZPropertyInfo(nameof(IsApplied)); }
		}

		#endregion

		#region TimeApplied

		[ReadOnly(true)]
		[XmlColumnProperty]
		public ZDateTime TimeApplied
		{
			get { return GetXmlColumnPropertyValue<ZDateTime>(TimeAppliedInfo); }
			set { SetXmlColumnPropertyValue(TimeAppliedInfo, value); }
		}

		public ZPropertyInfo TimeAppliedInfo
		{
			get { return GetZPropertyInfo(nameof(TimeApplied)); }
		}

		#endregion

		#region TimeRemoved

		[ReadOnly(true)]
		[XmlColumnProperty]
		public ZDateTime TimeRemoved
		{
			get { return GetXmlColumnPropertyValue<ZDateTime>(TimeRemovedInfo); }
			set { SetXmlColumnPropertyValue(TimeRemovedInfo, value); }
		}

		public ZPropertyInfo TimeRemovedInfo
		{
			get { return GetZPropertyInfo(nameof(TimeRemoved)); }
		}

		#endregion

		#endregion

		#region Validation

		public ShapeAffinityLinkValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected ShapeAffinityLinkValidation GetNewValidation()
		{
			return new ShapeAffinityLinkValidation(this);
		}

		#endregion

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}", IsApplied);
		}
	}
}
