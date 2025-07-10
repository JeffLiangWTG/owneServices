namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA12
{
	using System;
	using System.Text;
	using System.Text.RegularExpressions;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.GB.Business.Interfaces;
	using Enterprise.Customs.GB.MCP.ServiceTasks.RraProcessors.RRA12;

	public class RRA12ISLMessage : RRA12MessageBase
	{
		public override T CreateFromString<T>(string input)
		{
			/*
			 Format looks like this:
				SHAREDHEADER~NoNotLastMessage~lineItem1~lineItem2~lineItem3~lineItem4~lineItem5~....lineItem15~SHAREDFOOTER
				SHAREDHEADER~YesIsLastMessage~lineItem16~lineItem17~SHAREDFOOTER
			 That is, say, 17 line items split over two rows (blocks) in a file, each one starting with a shared header.
			 The last row int he file will identify itself as the final row.
			 We can have up to 15 line items per row.
			 */

			T rRA12ISLMessage = new T();

			int blockNumber = 0;
			foreach (ZString block in Regex.Split(input, Environment.NewLine))
			{
				if (blockNumber == 0)
				{
					string headerOnlyBlock = block.Left(lengthOfHeader);
					rRA12ISLMessage.Header = RRA12MessageHeader.CreateFromHeaderBlockISL(block);
				}
				if (!PrepareLinesReturningFalseWhenLastPopulatedItemSeen(block.SubstringSafe(lengthOfHeader), rRA12ISLMessage, block))
				{
					break;  // reached an empty line items... break
				}
				blockNumber++;
			}
			rRA12ISLMessage.PrettyText = MakePretty(rRA12ISLMessage);
			return rRA12ISLMessage;
		}

		ZString MakePretty(RRA12MessageBase msg)
		{
			string header = @"RRA12    Destin8       Amalgamation Clearance Advice          {0}

-----------------------------------------------------------------------------
UVI: {1}			             Actual Arrival: {2}
-----------------------------------------------------------------------------
 Agents Ref : {3} Entry No : {4}  {5}

 Agent : {6}

 UCN		B/L		Unit Id		Pkgs	Devan	Removal	Holds
{7}
";
			RRA12MessageHeader h = msg.Header;
			return string.Format(header, h.AdviceDate, h.UVI, h.ActualArrivalDate, h.AgentsReference, h.EntryNumber, h.EntryDate.ToString("dd/MM/yy"), h.Badge, WriteLinesPrettily(msg));
		}

		string WriteLinesPrettily(RRA12MessageBase msg)
		{
			StringBuilder sb = new StringBuilder();
			foreach (RRA12MessageLine line in msg.Lines)
			{
				sb.AppendLine(line.ToString());
			}
			return sb.ToString();
		}

		bool PrepareLinesReturningFalseWhenLastPopulatedItemSeen(ZString upToFifteenLineItemsBlockPlusFooter, RRA12MessageBase rRA12ISLMessage, ZString oneWholeBlock)
		{
			int totalElements = numberOfItemsPerAmalgamation * elementsPerLineItem;
			string[] elements = Regex.Split(upToFifteenLineItemsBlockPlusFooter, elementSeparator);
			int indexOfItemWithinBlock = 0;
			for (int indexOfElementWithinEntireBlock = 0; indexOfElementWithinEntireBlock < totalElements; indexOfElementWithinEntireBlock += elementsPerLineItem)
			{
				string ucn = elements[indexOfElementWithinEntireBlock + 0];
				string cont = elements[indexOfElementWithinEntireBlock + 1];
				string bol = elements[indexOfElementWithinEntireBlock + 2];
				string nop = elements[indexOfElementWithinEntireBlock + 3];
				string devan = elements[indexOfElementWithinEntireBlock + 4];
				string holds = elements[indexOfElementWithinEntireBlock + 5];
				if (!string.IsNullOrEmpty(ucn.Trim()))
				{
					RRA12MessageLine rRA12MessageLine = new RRA12MessageLine(ucn, bol, cont, nop, devan, holds);
					rRA12ISLMessage.Lines.Add(rRA12MessageLine);
					SetHoldFlagsAsComments(elements[indexOfElementWithinEntireBlock + 6], rRA12MessageLine);
					try
					{
						ZString fullBol = UpdateBolUsingFullVersionAfterItemsBlock(upToFifteenLineItemsBlockPlusFooter, oneWholeBlock, indexOfItemWithinBlock);
						if (!fullBol.IsEmpty)
						{ rRA12MessageLine.BoL = fullBol; }
					}
					catch (Exception ex) when (!ex.IsCriticalException()) { } //pre-2010 file, no prob
				}
				else
				{
					// No more items...
					return false;
				}
				indexOfItemWithinBlock++;
			}
			return true;
		}

		string UpdateBolUsingFullVersionAfterItemsBlock(ZString upToFifteenLineItemsBlockPlusFooter, ZString oneWholeBlock, int indexOfElementWithinItem)
		{
			var lengthOfUsernameAndGenrationBlock = 3 + 1 + 20;
			var lengthOfHeaderAndAllItems = lengthOfHeader + (numberOfItemsPerAmalgamation * lengthOfEachItem) + lengthOfUsernameAndGenrationBlock + 1; // 1024
			var bolsBlock = oneWholeBlock.Substring(lengthOfHeaderAndAllItems);
			ZString bolPossiblyWithEndOfFileMarker = Regex.Split(bolsBlock, elementSeparator)[indexOfElementWithinItem];
			return bolPossiblyWithEndOfFileMarker.Left(35).TrimEnd();
		}

		void SetHoldFlagsAsComments(string holdFlags, RRA12MessageLine rRA12MessageLine)
		{
			int numberOfFlags = 11;
			for (int flagIndex = 0; flagIndex < numberOfFlags; flagIndex++)
			{
				if (holdFlags[flagIndex] == yes)
				{
					SetLineCommentFromFlag(rRA12MessageLine, flagIndex);
				}
			}
		}

		void SetLineCommentFromFlag(RRA12MessageLine rRA12MessageLine, int flagIndex)
		{
			switch (flagIndex)
			{
				case 0:
					rRA12MessageLine.Comments.Add(PortHoldAuthorities.Descriptions.SubjectToPortHealthDetain);
					break;

				case 1:
					rRA12MessageLine.Comments.Add(PortHoldAuthorities.Descriptions.LocalCustomsHoldApplied);
					break;

				case 2:
					rRA12MessageLine.Comments.Add(PortHoldAuthorities.Descriptions.DOTHoldApplied);
					break;

				case 3:
					rRA12MessageLine.Comments.Add(PortHoldAuthorities.Descriptions.MAFFHoldApplied);
					break;

				case 4:
					rRA12MessageLine.Comments.Add(PortHoldAuthorities.Descriptions.ScannerHoldApplied);
					break;
			}
		}

		readonly char yes = 'Y';
		readonly int lengthOfHeader = 117;
		readonly int elementsPerLineItem = 7;
		readonly int numberOfItemsPerAmalgamation = 15;
		readonly int lengthOfEachItem = 61; // includes trailing ~
		internal readonly static string elementSeparator = "~";
	}
}
