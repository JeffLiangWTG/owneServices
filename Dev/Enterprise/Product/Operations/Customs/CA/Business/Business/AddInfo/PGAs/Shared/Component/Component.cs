using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CA.Business
{
	public class Component : AutoComponent
	{
		public Component(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Overrides

		public override bool SupportsNotes
		{
			get { return false; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(ComponentAddInfoLookups.UnitList))]
		public override ZString CA_UQ
		{
			get { return base.CA_UQ; }

			set { base.CA_UQ = value; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(ComponentAddInfoLookups.ComponentTypes))]
		public override ZString CA_Type
		{
			get => base.CA_Type;
			set => base.CA_Type = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(ComponentAddInfoLookups.SpecificationList))]
		public override ZString CA_Name
		{
			get => base.CA_Name;
			set => base.CA_Name = value;
		}

		#endregion

		#region New Properties

		public IPGAHeader PGAHeader
		{
			get
			{
				if (fPGAHeader == null)
				{
					fPGAHeader = Parent as IPGAHeader;
				}
				return fPGAHeader;
			}
		}
		IPGAHeader fPGAHeader;

		public ZString CA_NameFieldType
		{
			get
			{
				var cnscHeader = PGAHeader as CNSCPGAHeader;
				return (cnscHeader?.IsControlledSubstance ?? false) ? nameof(FieldType.TextDropEdit) : nameof(FieldType.Text);
			}
		}

		public ZString TypeDescription
		{
			get
			{
				if (typeDescriptionCached == null)
				{
					typeDescriptionCached = new CachedProperty<ZString>(Factory, () => AddInfoLookups.ComponentTypes.GetDescriptionFromCode(CA_Type));
				}

				return typeDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> typeDescriptionCached;

		public ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDescription)); }
		}

		#endregion
	}
}
