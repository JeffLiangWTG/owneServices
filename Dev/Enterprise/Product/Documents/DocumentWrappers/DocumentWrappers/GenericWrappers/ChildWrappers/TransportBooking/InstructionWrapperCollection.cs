using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class InstructionWrapperCollection : GenericWrapperCollection
	{
		#region Constructors

		public InstructionWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public InstructionWrapperCollection(BusinessObjectCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		#endregion

		#region this

		public new InstructionWrapper this[int index]
		{
			get { return (InstructionWrapper)base[index]; }
		}

		public new InstructionWrapper this[string index]
		{
			get
			{
				InstructionWrapper result = null;

				if (index == FirstPickup)
				{
					result = this.Cast<InstructionWrapper>().FirstOrDefault(i => IsMatch(i, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp));
				}
				else if (index == LastDelivery)
				{
					result = this.Cast<InstructionWrapper>().FirstOrDefault(i => IsMatch(i, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery));
				}
				else
				{
					result = (InstructionWrapper)base[index];
				}

				return result;
			}
		}

		ZBool IsMatch(InstructionWrapper instructionWrapper, ZString instructionType, ZString confirmationType)
		{
			var confirmation = instructionWrapper.ActionBO;
			var instruction = confirmation?.ConsignmentAddress;

			return instruction != null && instruction.ConsignmentAddressType == instructionType && confirmation.ActionType == confirmationType;
		}

		const string FirstPickup = "FirstPickup";
		const string LastDelivery = "LastDelivery";

		#endregion

		#region AddNew

		public new InstructionWrapper AddNew()
		{
			return (InstructionWrapper)base.AddNew();
		}

		#endregion
	}
}
