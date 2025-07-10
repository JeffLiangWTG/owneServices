using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Description")]
	[WrapperTypeName("Rating Direction")]
	public class RatingDirectionWrapper : GenericWrapper
	{
		public RatingDirectionWrapper(bool isOriginLocal, bool isDestinationLocal, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.IsOriginLocal = isOriginLocal;
			this.IsDestinationLocal = isDestinationLocal;
		}

		public ZBool IsOriginLocal { get; private set; }
		public ZBool IsDestinationLocal { get; private set; }

		public ZString Code
		{
			get
			{
				if (IsOriginLocal)
				{
					return IsDestinationLocal ? "DOM" : "EXP";
				}
				else
				{
					return IsDestinationLocal ? "IMP" : "CXT";
				}
			}
		}

		public ZString Description
		{
			get
			{
				if (IsOriginLocal)
				{
					if (IsDestinationLocal)
					{
						return Res.GetString("d41fd082-f665-4f46-aed4-84b8e51066aa", "Domestic");
					}
					else
					{
						return Res.GetString("c708a2f0-5f8e-41a6-bdef-0403ceb41ea6", "Export");
					}
				}
				else
				{
					if (IsDestinationLocal)
					{
						return Res.GetString("3928ef96-04df-41d0-b1f3-7c8040c2e22c", "Import");
					}
					else
					{
						return Res.GetString("e3573d18-9564-49c0-8f74-70b5f6ee0e4b", "Cross Trade");
					}
				}
			}
		}
	}
}
