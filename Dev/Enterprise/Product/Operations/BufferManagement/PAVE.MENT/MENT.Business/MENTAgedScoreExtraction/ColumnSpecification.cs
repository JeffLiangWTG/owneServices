using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.PAVE.MENT.Business
{
	public abstract class ColumnSpecification : NonPersistentBusinessObject<ColumnSpecificationValidation>
	{
		protected ColumnSpecification(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected ColumnSpecification()
		{
		}

		[XmlColumnProperty]
		[ResourceStringData("ColumnSpecification.Selected", Caption = "Selected", FullDescription = "Whether the selected column should be included.")]
		public ZBool Selected
		{
			get { return GetXmlColumnPropertyValue<ZBool>(SelectedInfo); }
			set
			{
				SetXmlColumnPropertyValue(SelectedInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSequence();
				}
			}
		}

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(nameof(Selected)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("ColumnSpecification.Column", Caption = "Column")]
		public virtual ZString Column
		{
			get { return GetXmlColumnPropertyValue<ZString>(ColumnInfo); }
			set { SetXmlColumnPropertyValue(ColumnInfo, value); }
		}

		public ZPropertyInfo ColumnInfo
		{
			get { return GetZPropertyInfo(nameof(Column)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("ColumnSpecification.Sequence", Caption = "Sequence", FullDescription = "The sequence the selected column should be part of the series")]
		public ZInt Sequence
		{
			get { return GetXmlColumnPropertyValue<ZInt>(SequenceInfo); }
			set
			{
				SetXmlColumnPropertyValue(SequenceInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSequence();
				}
			}
		}

		public ZPropertyInfo SequenceInfo
		{
			get { return GetZPropertyInfo(nameof(Sequence)); }
		}

		#region Validation

		public override ColumnSpecificationValidation GetNewValidation()
		{
			return new ColumnSpecificationValidation(this);
		}

		#endregion
	}
}
