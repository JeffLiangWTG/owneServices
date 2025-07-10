using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("INCO Term")]
	public class IncoTermWrapper : CodeAndDescriptionWrapper
	{
		#region Ctor

		public IncoTermWrapper(ZString code, IList list, BusinessObjectFactory factory)
			: this(code, list, null, factory)
		{
		}

		public IncoTermWrapper(ZString codeOverride, ZString descriptionOverride, IncoTermWrapper wrapperToCopy)
			: base(codeOverride, descriptionOverride, wrapperToCopy.Factory)
		{
			isCollectDeciderFunc = wrapperToCopy.isCollectDeciderFunc;
		}

		public IncoTermWrapper(ZString code, IList list, Func<string, bool?> isCollectDeciderFunc, BusinessObjectFactory factory)
			: base(code, list, factory)
		{
			this.isCollectDeciderFunc = isCollectDeciderFunc;
		}

		#endregion

		#region Nested Types

		public static class Deciders
		{
			public static Func<string, bool?> ByPaymentType(ZString paymentType)
			{
				return (incoCode) =>
					{
						bool? result = null;

						switch (paymentType)
						{
							case Core.Constants.PaymentType.Collect:
								result = true;
								break;
							case Core.Constants.PaymentType.Prepaid:
								result = false;
								break;
						}

						return result;
					};
			}

			public static Func<string, bool?> ByChargeGroup(ZString chargeGroupCode)
				=> incoCode =>
				{
					var prepaidCollect = IncoTermRegistry.GetPrepaidCollect(chargeGroupCode, incoCode);
					return prepaidCollect.In(Core.Constants.PaymentType.Collect, Core.Constants.PaymentType.Prepaid)
						? prepaidCollect == Core.Constants.PaymentType.Collect
						: null;
				};
		}

		#endregion

		#region Properties / Fields

		readonly Func<string, bool?> isCollectDeciderFunc;

		internal static new IncoTermWrapper Empty
		{
			get { return new IncoTermWrapper(CodeAndDescriptionWrapper.Empty.Code, CodeAndDescriptionWrapper.Empty.List, null, CodeAndDescriptionWrapper.Empty.Factory); }
		}

		public CodeAndDescriptionWrapper PaymentType
		{
			get
			{
				if (paymentType == null)
				{
					ZString code = ZString.Empty;

					bool? isCollect = isCollectDeciderFunc != null ? isCollectDeciderFunc(Code) : null;

					if (isCollect.HasValue && isCollect.Value)
					{
						code = Core.Constants.PaymentType.Collect;
					}
					else if (isCollect.HasValue && !isCollect.Value)
					{
						code = Core.Constants.PaymentType.Prepaid;
					}

					paymentType = new CodeAndDescriptionWrapper(code, new CodeDescriptionPairList(OLookUpEditType.PaymentType), Factory);
				}

				return paymentType;
			}
		}

		CodeAndDescriptionWrapper paymentType;

		#endregion
	}
}
