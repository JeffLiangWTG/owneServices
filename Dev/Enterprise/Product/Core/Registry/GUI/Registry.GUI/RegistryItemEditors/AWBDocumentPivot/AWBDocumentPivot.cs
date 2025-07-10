using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	public class AWBDocumentPivot : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AWBDocumentPivot(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Name

		[CargoWise.ComponentModel.MaxLength(250)]
		public ZString Name
		{
			get { return name; }
			set
			{
				CheckMaximumLength(NameInfo, value);
				SetNonPersistentPropertyValue<ZString>(NameInfo, ref name, value);
			}
		}

		ZString name;

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		#endregion

		#region Title

		[CargoWise.ComponentModel.MaxLength(250)]
		public ZString Title
		{
			get { return title; }
			set
			{
				if (title != value)
				{
					CheckMaximumLength(TitleInfo, value);
					SetNonPersistentPropertyValue<ZString>(TitleInfo, ref title, value);
				}
			}
		}

		ZString title;

		public ZPropertyInfo TitleInfo
		{
			get { return GetZPropertyInfo(nameof(Title)); }
		}

		#endregion

		#region Printed

		public ZBool Printed
		{
			get { return printed; }
			set { SetNonPersistentPropertyValue<ZBool>(PrintedInfo, ref printed, value); }
		}

		ZBool printed;

		public ZPropertyInfo PrintedInfo
		{
			get { return GetZPropertyInfo(nameof(Printed)); }
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("b798fe41-4e9d-489f-b249-21f3d3c66dd6", "Use the 'Printed?' column to enable/disable printing of this Document"); }
		}

		#endregion
	}
}