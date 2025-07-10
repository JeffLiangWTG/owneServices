using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusContainer : BaseCusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("F966750A-5B58-47C9-A262-013434C4E943", Caption = "NACCS Container Type", MediumCaption = "NACCS Cont. Type", ShortCaption = "NACCS Type")]
		public ZString ContainerType => Factory.GetValue(ref containerType, () => ContainerHelper.GetNACCSContainerType(Container));
		CachedProperty<ZString> containerType;

		[ResourceStringData("27C946AE-4D59-489A-856B-3495814C747D", Caption = "NACCS Container Size", MediumCaption = "NACCS Cont. Size", ShortCaption = "NACCS Size")]
		public ZString ContainerSize => Factory.GetValue(ref containerSize, () => ContainerHelper.GetNACCSContainerSize(Container));
		CachedProperty<ZString> containerSize;

		[ChildEditable]
		public CusSealCollection AdditionalSeals
		{
			get
			{
				if (additionalSeals == null)
				{
					additionalSeals = new CusSealCollection(this);
					RegisterEditableChildObject(additionalSeals);
				}

				return additionalSeals;
			}
		}
		CusSealCollection additionalSeals;

		internal ShortSequenceNumberGenerator SealsSequenceNumberGenerator => sealsSequenceNumberGeneratorCache ??= new ShortSequenceNumberGenerator(() => AdditionalSeals);
		ShortSequenceNumberGenerator sealsSequenceNumberGeneratorCache;

		ContainerHelper ContainerHelper => containerHelper ??=  new ContainerHelper(Factory);
		ContainerHelper containerHelper;
	}
}
