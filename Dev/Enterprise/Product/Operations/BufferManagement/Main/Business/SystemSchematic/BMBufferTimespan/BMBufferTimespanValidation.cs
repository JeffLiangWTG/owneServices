using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMBufferTimespanValidation : AutoBMBufferTimespanValidation
	{
		public BMBufferTimespanValidation(AutoBMBufferTimespan parent)
			: base(parent)
		{
		}

		new BMBufferTimespan Parent => (BMBufferTimespan)base.Parent;

		protected override void CheckBMT_Name()
		{
			base.CheckBMT_Name();
			MandatoryValidation.CheckEntered(Parent.BMT_NameInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.BMT_NameInfo, new BMBufferTimespanCollection(Parent.Factory));
		}

		protected override void CheckBMT_BufferTimespanInMinutes()
		{
			base.CheckBMT_BufferTimespanInMinutes();

			MandatoryValidation.CheckEntered(Parent.BMT_BufferTimespanInMinutesInfo);
			if (Parent.BMT_BufferTimespanInMinutes < 3)
			{
				Parent.BMT_BufferTimespanInMinutesInfo.AddError(Res.GetString("efef6ee4-8948-4779-9ddb-5703727c164f", "The buffer timespan should be at least three minutes, since there are always three zones in a buffer, which are each one minute at the smallest."));
			}
		}
	}
}
