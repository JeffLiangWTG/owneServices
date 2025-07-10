namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class InstructionWrapperTest : Base.Testing.GenericWrapperTest
	{
		#region ExpectedFieldMap

		protected override string ExpectedFieldMap
		{
			get
			{
				return Wrapper.GetType().Name.Replace("Wrapper", "") + @"
======================================================================
Name                                    Type
----------------------------------------------------------------------
Address                                 Address
DropMode                                CodeAndDescription
Transport                               Freight
RequiredFrom                            LabelValuePair
RequiredTo                              LabelValuePair
Volume                                  Volume
Weight                                  Weight
ConfirmationDescription                 String
ConfirmationID                          String
ConfirmationQuantity                    Int
ConfirmationReferenceNum                String
ConfirmationType                        String
ConsignorOrConsigneeAddress             String
Equipment                               String
EstimatedOrSlot                         DateTime
HasSingleConfirmationForWholePackage    Bool
InstructionType                         String
PackageDimensions                       String
PackageDivotID                          String
PackageDivotQuantity                    Int
PackageDivotSequence                    Int
PackageID                               String
PackageType                             String
ReceivedBy                              String
Sequence                                Int
ServiceInstruction                      String
Status                                  String
TimeIn                                  DateTime
TimeOut                                 DateTime
UNDGsSummary                            String

";
			}
		}

		#endregion

		#region DefaultDropModes

		protected CodeAndDescriptionWrapper[] DefaultDropModes
		{
			get
			{
				return new[]
				{
					new CodeAndDescriptionWrapper("ANY", "Any", Factory),
					new CodeAndDescriptionWrapper("ASK", "Ask Client", Factory),
					new CodeAndDescriptionWrapper("HSL", "Haulier Supplies Lift", Factory),
					new CodeAndDescriptionWrapper("HUL", "Hand Unload/Load by Premise", Factory),
					new CodeAndDescriptionWrapper("HWL", "Hand Unload/Load by Haulier", Factory),
					new CodeAndDescriptionWrapper("LOF", "Drop Container - Premise supplies Lift", Factory),
					new CodeAndDescriptionWrapper("PSL", "Premise Supplies Lift", Factory),
					new CodeAndDescriptionWrapper("SDL", "Drop Container with Sideloader", Factory),
					new CodeAndDescriptionWrapper("TRL", "Drop Trailer", Factory),
					new CodeAndDescriptionWrapper("WUP", "Wait for Pack/Unpack", Factory)
				};
			}
		}

		#endregion
	}
}
