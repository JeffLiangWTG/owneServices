using System;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAOUTMessageLineSentToCustoms : UniqueIdentifierMessageLine
	{
		public SEAOUTMessageLineSentToCustoms(SegmentGroup7 group7)
		{
			this.group7 = group7;
			ProcessSegmentGroup(group7, out ContainerNo, out OceanBill, out HouseBill, out LineAction);
		}
		readonly SegmentGroup7 group7;
		public readonly ZString ContainerNo;
		public readonly ZString OceanBill;
		public readonly ZString HouseBill;
		public readonly string LineAction;

		void ProcessSegmentGroup(SegmentGroup7 group7, out ZString containerNo, out ZString oceanBill, out ZString houseBill, out string lineAction)
		{
			containerNo = string.Empty;
			houseBill = string.Empty;
			oceanBill = string.Empty;
			lineAction = group7.CNI.Count > 0 && group7.CNI[0].DocumentMessageDetails != null ? group7.CNI[0].DocumentMessageDetails.LanguageNameCode : string.Empty;
			foreach (SegmentGroup8 group8 in group7.Group8)
			{
				var reference = group8.RFF.Count > 0 ? group8.RFF[0].Reference : null;
				if (reference != null)
				{
					if (reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.UnitLoadDeviceEGContainerIdentificationNumber)
					{
						containerNo = reference.ReferenceIdentifier;
					}
					if (reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.HouseBillOfLadingNumber)
					{
						houseBill = reference.ReferenceIdentifier;
					}
					if (reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.MasterBillOfLadingNumber)
					{
						oceanBill = reference.ReferenceIdentifier;
					}
				}
			}
		}

		public override void Populate(SegmentGroup segmentGroup, string lineActionCode)
		{
			SegmentGroup7 newGroup7 = (SegmentGroup7)segmentGroup;
			newGroup7.Parse(CharacterSet, group7.ToString(CharacterSet));
			MessageUtilities.PopulateCNI(newGroup7.CNI[0], null, lineActionCode);
		}

		UNOCCMRCharacterSet CharacterSet
		{
			get { return fCharacterSet ?? (fCharacterSet = new UNOCCMRCharacterSet()); }
		}
		UNOCCMRCharacterSet fCharacterSet;

		public override string UniqueIdentifier
		{
			get { return SEAOUTMessageLine.GetUniqueIdentifier(ContainerNo, OceanBill, HouseBill); }
		}

		public override SegmentGroup GetNewSegmentGroup(SegmentGroup message)
		{
			return ((CUSCARMessage)message).Group7.InstantiateAChildAndAddItToChildrenCollection();
		}

		protected internal override Type SegmentGroupType => typeof(SegmentGroup7);
	}
}
