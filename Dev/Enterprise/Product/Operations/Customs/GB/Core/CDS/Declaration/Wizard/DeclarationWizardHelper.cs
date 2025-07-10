using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.CDS
{
	public class DeclarationWizardHelper
	{
		public DeclarationWizardItem[] ReadCsvString(string[] csvString)
		{
			return CreateFromString(csvString);
		}

		public DeclarationWizardItem[] ReadCsvString()
		{
			return CreateFromString(csvData);
		}

		static DeclarationWizardItem[] CreateFromString(string[] csvString)
		{
			return (from line in csvString.Skip(1)
					let columns = SplitCSV(line).ToArray()
					select new DeclarationWizardItem
					{
						RequestedProcedure = columns[0].Replace("\"", "").Trim(),
						ProcedureDefinition = columns[1].Replace("\"", "").Trim(),
						Q2DeclarationType = columns[2].Replace("\"", "").Trim(),
						Q3 = columns[3].Replace("\"", "").Trim(),
						DeclarationType = columns[4].Replace("\"", "").Trim(),
						Q1 = columns[5].Replace("\"", "").Trim(),
						AdditionalDeclarationType = columns[6].Replace("\"", "").Trim(),
						ProcedureCategory = columns[7].Replace("\"", "").Trim()
					}).ToArray();
		}

		static IEnumerable<string> SplitCSV(string input)
		{
			Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

			foreach (Match match in csvSplit.Matches(input))
			{
				yield return match.Value.TrimStart(',');
			}
		}

		public static bool IsImport(ZString procedureCategory) => new ImportDeclarationTypeList().ContainsCode(procedureCategory);

		readonly string[] csvData = new string[] { @"Requested Procedure (D.E. 1/10),Procedure definition,Question 2 - declaration type,Question 3 (only needed for #2='FC'), ""Declaration Type(D.E 1/1)"",Question 1 - IM, EX or CO, Additional Declaration Type (D.E. 1/2),Procedure Category",
			@"00,Clearance request,FC,None,IM,IM,""J or K"",21I",
@"00,Clearance request,FC,None,IM,CO,""J or K"",21I",
@"01,Release for free circulation with simultaneous onward dispatch to another Customs Union Territory,FC,OD,IM,IM,""A, D or Y"",H1",
@"01,Release for free circulation with simultaneous onward dispatch to another Customs Union Territory,FC,OD,IM,IM,C or F ,I1",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,IM ,IM,""A, D or Y"",H1",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,CO,CO,""A, D or Y"",H5",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,IM or CO,IM,C or F ,I1",
@"07,Release for Free Circulation with simultaneous Entry to an Excise Warehouse,FC,EXC,IM or CO,CO,C or F ,I1",
@"40,Release to free circulation, FC, None, IM, IM,""A, D, Y or Z"",H1",
@"40,Release to free circulation ,FC,None,CO,CO,""A, D, Y or Z"",H5",
@"40,Release to free circulation, FC, None, IM or CO, IM,""B, C, E or F"", I1",
@"40,Release to free circulation ,FC,None,IM or CO,CO,""B, C, E or F"",I1",
@"40,Clearance request,FC,None,IM,IM,""J or K"",21I",
@"40,Clearance request,FC,None,IM,CO,""J or K"",21I",
@"42,Release for free circulation with simultaneous onward supply to another member state, FC, OSR, IM, IM,""A, D or Y"", H1",
@"42,Release for free circulation with simultaneous onward supply to another member state, FC, OSR, IM, IM, C or F, I1",
@"44,Release to free circulation with duty relief granted under the End Use Special Procedure,FC,End Use, IM, IM,""A, D, Y or Z"", H1",
@"44,Release to free circulation with duty relief granted under the End Use Special Procedure,FC,End Use, IM, IM,""B, C, E or F"", I1",
@"44,Clearance request,FC,None,IM,IM,""J or K"",21I",
@"44,Clearance request,FC,None,IM,CO,""J or K"",21I",
@"51,Entry to Inward Processing,IP,,IM or CO,IM,""A, D, Y or Z"",H4",
@"51,Entry to Inward Processing, IP,, IM or CO, CO,""A, D, Y or Z"",H4",
@"51,Entry to Inward Processing,IP,,IM or CO,IM,C or F,I1",
@"51,Entry to Inward Processing, IP,, IM or CO, CO, C or F,I1",
@"51,Clearance request,FC,None,IM,IM,""J or K"",21I",
@"51,Clearance request,FC,None,IM,CO,""J or K"",21I",
@"53,Entry to Temporary Admission,TA,,IM or CO ,IM,""A, D, Y or Z"",H3",
@"53,Entry to Temporary Admission, TA,, IM or CO, CO,""A, D, Y or Z"",H3",
@"53,Entry to Temporary Admission,TA,,IM or CO ,IM,C or F,I1",
@"53,Entry to Temporary Admission, TA,, IM or CO, CO, C or F, I1",
@"53,Clearance request,FC,None,IM,IM,""J or K"",21I",
@"53,Clearance request,FC,None,IM,CO,""J or K"",21I",
@"61,Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,IM, IM,""A, D, Y or Z"",H1",
@"61,Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,CO,CO,""A, D, Y or Z"",H5",
@"61,Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,IM or CO,IM,C or F,I1",
@"61,Re-importation with simultaneous release to Free Circulation,FC,Re-Imp,IM or CO,CO,C or F,I1",
@"61,Clearance request,FC,None,IM,IM,""J or K"",21I",
@"61,Clearance request,FC,None,IM,CO,""J or K"",21I",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO,IM,A or D,H2",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO,CO,A or D,H2",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO,IM,C or F,I1",
@"71,Entry to a Customs Warehouse(CW),CW,,IM or CO,CO,C or F,I1",
@"71,Clearance request,FC,None,IM,IM,""J or K"",21I",
@"71,Clearance request,FC,None,IM,CO,""J or K"",21I",
@"1007,Permanent Export from an excise warehouse,PE,EXC,EX,EX,""A, D or Y"",B1",
@"1040,Permanent Export,PE,None,EX,EX,""A, D, Y or Z"",B1",
@"1044,Permanent Export after end use,PE,End Use,EX,EX,""A, D, Y or Z"",B1",
@"1042,Permanent Export after onward supply relief,PE,OSR,EX,EX,""A, D or Y"",B1",
@"1007,Permanent Export from an excise warehouse,PE,EXC,CO,CO,""A, D or Y"",B4",
@"1040,Permanent Export,PE,None,CO,CO,""A, D, Y or Z"",B4",
@"1042,Permanent Export after onward supply,PE,OSR,CO,CO,""D or Y"",B4",
@"1007,Permanent Export from an excise warehouse,PE,EXC,EX or CO,EX,""F"",C1",
@"1007,Permanent Export from an excise warehouse,PE,EXC,EX or CO,CO,""C or F"",C1",
@"1042,Permanent Export after onward supply relief,PE,OSR,EX or CO,EX,""F"",C1",
@"1042,Permanent Export after onward supply relief,PE,OSR,EX or CO,CO,""C or F"",C1",
@"1040,Permanent Export,PE,None,EX or CO,EX,""B, C, E or F"",C1",
@"1040,Permanent Export,PE,None,EX or CO,CO,""C, E or F"",C1",
@"1044,Permanent Export after end use,PE,End Use,EX,EX,""C or F"",C1",
@"1100,Inward Processing (IP) with Prior Export Equivalence (PEE),IPPEE,,EX,EX,""A, D or Y"",B1",
@"1100,Inward Processing (IP) with Prior Export Equivalence (PEE),IPPEE,,CO,CO,""A, D or Y"",B4",
@"1100,Inward Processing (IP) with Prior Export Equivalence (PEE),IPPEE,,EX or CO,CO,""C or F"",C1",
@"1100,Inward Processing (IP) with Prior Export Equivalence (PEE),IPPEE,,EX or CO,EX,""C or F"",C1",
@"2100,Temporary Export under Outward Processing (OP),OP1,None,EX,EX,""A or D"",B2",
@"2144,Temporary Export under Outward Processing (OP) after end use,OP1,End Use,EX,EX,""A or D"",B2",
@"2151,Temporary Export under Outward Processing (OP) after inward processing,OP1,IP,EX,EX,""A or D"",B2",
@"2154,Temporary Export under Outward Processing (OP) after inward processing in EU,OP1,IPEU,EX,EX,""A or D"",B2",
@"2200,Temporary Export under Outward Processing (OP) if not covered by Requested Procedure Code 21,OP2,None,EX or CO,EX,""A or D"",B2",
@"2200,Temporary Export under Outward Processing (OP) if not covered by Requested Procedure Code 21,OP2,None,EX or CO,CO,""A or D"",B2",
@"2244,Temporary Export under Outward Processing (OP) if not covered by Requested Procedure Code 21 after end use,OP2,End Use,EX or CO,EX,""A or D"",B2",
@"2244,Temporary Export under Outward Processing (OP) if not covered by Requested Procedure Code 21 after end use,OP2,End Use,EX or CO,CO,""A or D"",B2",
@"2300,Returned Goods Relief (RGR),RGR,,EX,EX,""A, D, Y or Z"",B1",
@"2300,Returned Goods Relief (RGR),RGR,,CO,CO,""A, D, Y or Z"",B4",
@"2300,Returned Goods Relief (RGR),RGR,,EX or CO,CO,""F"",C1",
@"2300,Returned Goods Relief (RGR),RGR,,EX or CO,EX,""F"",C1",
@"3151,Re-export of non-Union goods after inward processing,Re-Exp,IP,EX,EX,""A, D, Y or Z"",B1",
@"3153,Re-export of non-Union goods after temporary admission,Re-Exp,TA,EX,EX,""A, D, Y or Z"",B1",
@"3171,Re-export of non-Union goods after customs warehousing,Re-Exp,CW,EX,EX,""A, D, Y or Z"",B1",
@"3154,Re-export of non-Union goods inward processing in EU,Re-Exp,IPEU,EX,EX,""A or D"",B1",
@"3151,Re-export of non-Union goods after inward processing,Re-Exp,IP,CO,CO,""A, D, Y or Z"",B4",
@"3153,Re-export of non-Union goods after temporary admission,Re-Exp,TA,CO,CO,""A, D, Y or Z"",B4",
@"3171,Re-export of non-Union goods after customs warehousing,Re-Exp,CW,CO,CO,""A, D, Y or Z"",B4",
@"3151,Re-export of non-Union goods after inward processing,Re-Exp,IP,EX or CO,CO,""C or F"",C1",
@"3153,Re-export of non-Union goods after temporary admission,Re-Exp,TA,EX or CO,CO,""C or F"",C1",
@"3171,Re-export of non-Union goods after customs warehousing,Re-Exp,CW,EX or CO,CO,""C or F"",C1",
@"3151,Re-export of non-Union goods after inward processing,Re-Exp,IP,EX or CO,EX,""C or F"",C1",
@"3153,Re-export of non-Union goods after temporary admission,Re-Exp,TA,EX or CO,EX,""C or F"",C1",
@"3171,Re-export of non-Union goods after customs warehousing,Re-Exp,CW,EX or CO,EX,""C or F"",C1" };
	}
}
