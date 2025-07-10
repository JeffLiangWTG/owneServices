using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Edifact.Utilities;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class IMDTransportLine
	{
		public IMDTransportLine(JobDeclaration declaration, SegmentGroup21 group21)
		{
			this.Declaration = declaration;
			this.group21 = group21;
		}

		public abstract void Populate(int lineNumber, string lineActionCode);

		#region LIN Segment

		protected void PopulateLIN(ZInt lineNumber, ZString lineActionCode)
		{
			LINSegment lIN = group21.LIN.InstantiateAChildAndAddItToChildrenCollection();
			lIN.ActionRequestNotificationDescriptionCode = ActionRequestNotificationDescriptionCodeList.GetFromString(lineActionCode);
			lIN.LineItemNumber = lineNumber.ToString();
		}

		#endregion

		#region GIS Segment

		protected void PopulateGIS()
		{
			if (Declaration != null && Declaration.AddInfo.ZA_VIS_Hidden)
			{
				GISSegment gIS = group21.GIS.InstantiateAChildAndAddItToChildrenCollection();
				gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString("VIS");
				gIS.ProcessingIndicator_X.CodeListIdentificationCode = CodeListIdentificationCodeList.CustomsIndicator;
				gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		#endregion

		#region Segment Group 28

		protected abstract void PopulateGroup28();

		#region Group 28

		protected internal SegmentGroup28 Group28
		{
			get { return fGroup28; }
			set { fGroup28 = value; }
		}
		SegmentGroup28 fGroup28;

		#endregion

		#region PAC Segment

		protected void PopulatePAC(ZString packageTypeDescriptionCode)
		{
			if (!packageTypeDescriptionCode.IsEmpty)
			{
				PACSegment cargoTypeSegment = Group28.PAC.InstantiateAChildAndAddItToChildrenCollection();
				cargoTypeSegment.PackageType.PackageTypeDescriptionCode = packageTypeDescriptionCode;
				cargoTypeSegment.PackageType.CodeListIdentificationCode = CodeListIdentificationCodeList.TypeOfPackage;
				cargoTypeSegment.PackageType.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		protected void PopulatePAC(ZInt packages, PackagingLevelCodedList packagingLevelCode)
		{
			if (packages > 0)
			{
				PACSegment packingUnitCountSegment = Group28.PAC.InstantiateAChildAndAddItToChildrenCollection();
				packingUnitCountSegment.NumberOfPackages = packages.ToString();
				packingUnitCountSegment.PackagingDetails.PackagingLevelCoded = packagingLevelCode;
			}
		}

		#endregion

		#endregion

		#region Segment Group 29

		protected abstract void PopulateGroup29();

		#region Group 29

		SegmentGroup29 Group29
		{
			get { return fGroup29; }
			set { fGroup29 = value; }
		}
		SegmentGroup29 fGroup29;

		#endregion

		#region PCI Segment

		protected void PopulatePCI(MarkingInstructionsCodedList markingInstructionsCode, ZString marksAndNumbers)
		{
			if (!marksAndNumbers.IsEmpty)
			{
				if (Group29 == null)
				{
					Group29 = Group28.Group29.InstantiateAChildAndAddItToChildrenCollection();
				}

				PCISegment pCI = Group29.PCI.InstantiateAChildAndAddItToChildrenCollection();

				pCI.MarkingInstructionsCoded = markingInstructionsCode;

				TextSplitter splitter = new TextSplitter(35);
				splitter.Text = marksAndNumbers.Trim().Replace("\r\n", " ");
				pCI.MarksLabels.ShippingMarks1 = splitter[0];
				pCI.MarksLabels.ShippingMarks2 = splitter[1];
				pCI.MarksLabels.ShippingMarks3 = splitter[2];
				pCI.MarksLabels.ShippingMarks4 = splitter[3];
				pCI.MarksLabels.ShippingMarks5 = splitter[4];
				pCI.MarksLabels.ShippingMarks6 = splitter[5];
				pCI.MarksLabels.ShippingMarks7 = splitter[6];
				pCI.MarksLabels.ShippingMarks8 = splitter[7];
				pCI.MarksLabels.ShippingMarks9 = splitter[8];
				pCI.MarksLabels.ShippingMarks10 = splitter[9];
			}
		}

		#endregion

		#region RFF Segment

		protected void PopulateRFF(ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, ZString referenceIdentifier)
		{
			if (!referenceIdentifier.IsEmpty)
			{
				SegmentGroup29 group29 = Group28.Group29.InstantiateAChildAndAddItToChildrenCollection();
				PCISegment pCI = group29.PCI.InstantiateAChildAndAddItToChildrenCollection();
				pCI.MarkingInstructionsCoded = MarkingInstructionsCodedList.DoNotMarkSuppliersCompanyName;

				RFFSegment masterBillRFF = group29.RFF.InstantiateAChildAndAddItToChildrenCollection();
				masterBillRFF.Reference.ReferenceFunctionCodeQualifier = referenceFunctionCodeQualifier;
				masterBillRFF.Reference.ReferenceIdentifier = referenceIdentifier;
			}
		}

		#endregion

		#endregion

		protected readonly JobDeclaration Declaration;
		protected internal SegmentGroup21 group21;
	}
}
