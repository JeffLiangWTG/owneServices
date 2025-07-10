using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(Schema.BMR_Name), DescriptionProperty(Schema.BMR_Name)]
	public class BMReleaseSequence : AutoBMReleaseSequence, IBMReleaseSequence
	{
		public BMReleaseSequence(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		public BMReleaseSequenceItemCollection Items
		{
			get
			{
				if (items == null)
				{
					items = new BMReleaseSequenceItemCollection(this);
					RegisterEditableChildObject(items);
				}

				return items;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			BMR_SequenceNudge = BMSRegistry.Instance.ReleaseSequenceDefaultNudge.Value;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		ZGuid IBMReleaseSequence.BMR_PK => PK;

		BMReleaseSequenceItemCollection items;
	}
}
