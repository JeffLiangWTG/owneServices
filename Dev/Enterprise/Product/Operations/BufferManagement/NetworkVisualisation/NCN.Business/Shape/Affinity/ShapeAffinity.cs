using System;
using System.Diagnostics;
using System.Drawing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using ColorList = CargoWise.NetworkVisualisation.Business.ColorList;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("{Name}")]
	public class ShapeAffinity : NonPersistentBusinessObject, IAffinity
	{
		public ShapeAffinity(BusinessObjectFactory factory)
			: base(factory)
		{
		}

#if DEBUG
		public ShapeAffinity(ZString color, ZString name, ZGuid pK, int allowedConcurrency = 1)
		{
			Name = name;
			Color = color;
			AffinityPK = pK;
			AllowedConcurrency = allowedConcurrency;
		}
#endif

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AllowedConcurrency = 1;
		}

		#endregion

		#region Properties

		#region AffinityPK

		[XmlColumnProperty]
		public ZGuid AffinityPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(AffinityPKInfo); }
			set { SetXmlColumnPropertyValue(AffinityPKInfo, value); }
		}

		public ZPropertyInfo AffinityPKInfo
		{
			get { return GetZPropertyInfo(nameof(AffinityPK)); }
		}

		#endregion

		#region Color

		[XmlColumnProperty]
		[List("ColorList")]
		[ResourceStringData("ShapeAffinity|Color", Caption = "Color", FullDescription = "The color which will be applied to the background of entities with this affinity.")]
		[MaxLength(50)]
		public ZString Color
		{
			get { return GetXmlColumnPropertyValue<ZString>(ColorInfo); }
			set
			{
				SetXmlColumnPropertyValue(ColorInfo, value);
				Validation.ValidateColor();
			}
		}

		public ZPropertyInfo ColorInfo
		{
			get { return GetZPropertyInfo(nameof(Color)); }
		}

		#endregion

		#region Name

		[XmlColumnProperty]
		[ResourceStringData("ShapeAffinity|Name", Caption = "Name")]
		[MaxLength(100)]
		public ZString Name
		{
			get { return GetXmlColumnPropertyValue<ZString>(NameInfo); }
			set
			{
				SetXmlColumnPropertyValue(NameInfo, value);
				Validation.ValidateName();
			}
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		#endregion

		#region AllowedConcurrency

		[XmlColumnProperty]
		[ResourceStringData("ShapeAffinity|AllowedConcurrency", Caption = "Allowed Concurrency", FullDescription = "The maximum number of entities with this affinity which can run concurrently, for leveling purposes.")]
		public ZInt AllowedConcurrency
		{
			get { return GetXmlColumnPropertyValue<ZInt>(AllowedConcurrencyInfo); }
			set
			{
				SetXmlColumnPropertyValue(AllowedConcurrencyInfo, value);
				Validation.ValidateName();
			}
		}

		public ZPropertyInfo AllowedConcurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(AllowedConcurrency)); }
		}

		#endregion

		#endregion

		#region IAffinity Members

		Color IAffinity.Colour
		{
			get => ColorFromNameConverter.ColorFromName(Color);
			set => Color = value.Name;
		}

		string IAffinity.Name
		{
			get { return Name; }
			set { Name = value; }
		}

		Guid IAffinity.AffinityPK
		{
			get { return AffinityPK.ToGuid(); }
			set { AffinityPK = value; }
		}

		int IAffinity.AllowedConcurrency
		{
			get { return AllowedConcurrency; }
			set { AllowedConcurrency = value; }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList ColorList
		{
			get { return colorList ?? (colorList = new ColorList().GetTranslatedColorNames()); }
		}

		CodeDescriptionPairList colorList;

		#endregion

		#region Validation

		public ShapeAffinityValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected ShapeAffinityValidation GetNewValidation()
		{
			return new ShapeAffinityValidation(this);
		}

		#endregion
	}
}
