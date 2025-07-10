namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA01AndRRA11
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.GB.Business;
	using Enterprise.Customs.GB.Business.Declaration;
	using Enterprise.Customs.GB.Business.HoldAdderRemoverClearer;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;
	using static System.FormattableString;

	/// <summary>
	/// Represents the bare necessities of an RRA11 message, which is a text file received from (e.g.) Destin8.  It's some kind of customs status update message.
	/// </summary>
	public class RRA01AndRRA11Message : IUcnProvider, IPortAuthorityHoldApplicationProvider
	{
		protected char delimiter = '~';  // tilde
		protected ZString messageReceivedFromCustoms;

		/// <summary>
		/// Constructor with long text that we will chop up and interpret
		/// </summary>
		public RRA01AndRRA11Message(ZString messageTextAsReceivedFromCustoms)
		{
			messageReceivedFromCustoms = messageTextAsReceivedFromCustoms;
		}

		#region Public and private properties of the RRA01 or RRA11 Message

		/// <summary>
		/// The berth in which the event occurred
		/// e.g. TTY represents Trinity Terminal
		/// The Berth code is the [1]st element
		/// </summary>
		public ZString BerthCode { get; set; }

		/// <summary>
		/// A unique consignment number.  Either 9, 12 or 14 chars. Stored as string.
		/// UCN is element [2] of the file
		/// </summary>
		public UniqueConsignmentNumber UCN { get; set; }

		//IUcnProvider:
		public ZString UcnNumberProperlyTruncated
		{
			get
			{
				return UCN.ProperlyTruncatedUCN;
			}
		}

		public ZString UcnNumberVerbatim
		{
			get { return UCN.Raw; }
		}

		/// <summary>
		/// The container in question
		/// </summary>
		public ZString Container { get; set; }

		/// <summary>
		/// The container in question's 2nd ID
		/// Second Container number, or rather "Unit ID 2", is the [4]th element of the file
		/// </summary>
		public ZString ContainerNumber2 { get; set; }

		/// <summary>
		/// The marks and numbers of the shipment
		/// Marks and numbers is the [6]th element of the file
		/// </summary>
		public ZString MarksAndNumbers { get; set; }

		/// <summary>
		/// NoP is element [7]
		/// </summary>
		public ZInt Packages { get; set; }

		// Weight is element [8]
		public ZDecimal Weight { get; set; }

		/// <summary>
		/// The date stamp that customs declare in the message.  May be different from the date that the message was created.
		/// Date is the [9]th element
		/// </summary>
		public ZDateTime DateOfStatusEvent { get; set; }

		/// <summary>
		/// Date that the message was created.  May be different from the date that the event occurred.
		/// </summary>
		public ZDateTime DateOfMessageReceipt { get; set; }

		/// <summary>
		/// Bill of lading upon which this event occurred
		/// BL is the [10]th element... and for ICS it is sent again in element 13 (0-based), with the full 35 characters.  If element 14 exists we will update BlNumber with it.
		/// </summary>
		public ZString BlNumber { get; set; }

		// We ignore element 11, the Generating transaction ID.  Advice of Amy @ MCP.

		/// <summary>
		/// The user responsible for this event (at the docks)
		/// Name is the [12]th element of the file
		/// </summary>
		public ZString UserName { get; set; }

		/// <summary>
		/// The type of RRA message (RRA01 or RRA11)
		/// </summary>
		public virtual ZString MessageType { get; protected set; }

		protected ZString MessageIdentifier
		{
			get
			{
				return messageReceivedFromCustoms.SubstringSafe(0, 6);
			}
		}

		protected virtual List<ZString> ValidMessageIdentifiers
		{
			get
			{
				return new List<ZString> { "RRA01", "RRA11" };
			}
			set
			{
				throw new NotSupportedException("Property can not be assigned to - it is read-only");
			}
		}

		protected ZString GetMessageIdentifierString(bool or, string prefix = "", string suffix = "")
		{
			var result = string.Empty;

			if (ValidMessageIdentifiers.Count == 1)
			{
				result = Invariant($"{prefix}{ValidMessageIdentifiers[0]}{suffix}");
			}
			else if (ValidMessageIdentifiers.Count == 2)
			{
				result = string.Join(string.Format(CultureInfo.InvariantCulture, "{0}", or ? " or " : " and "), ValidMessageIdentifiers.Select(x => Invariant($"{prefix}{x}{suffix}")));
			}
			else
			{
				var commaList = string.Join(", ", ValidMessageIdentifiers.Except(new ZString[] { ValidMessageIdentifiers.Last() }).Select(x => Invariant($"{prefix}{x}{suffix}")));
				result = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}{3}{4}", commaList, or ? "or" : "and", prefix, ValidMessageIdentifiers.LastOrDefault(), suffix);
			}

			return result;
		}

		protected virtual ZInt MessageLength => 13;

		protected ZString InvalidMessageException => Invariant($"This is the {GetMessageIdentifierString(false)} processor, and you passed me something that is not an {GetMessageIdentifierString(true)}. It did not start with {GetMessageIdentifierString(true, "'=", "'")}. It was [{messageReceivedFromCustoms}]");

		protected ZString MissingDelimiterException => Invariant($"This is the {GetMessageIdentifierString(false)} processor. The message that I was asked to parse did not contain any delimiters.");

		protected ZString MessageLengthException => Invariant($"This is the {GetMessageIdentifierString(false)} processor. The message that I was asked to parse did not contain the expected number of elements.");

		#region January 2013 / v3.5 additions

		public ZString AgentsReference { get; protected set; }
		public ZString CHIEFEntryEPU { get; protected set; }
		public ZString CHIEFEntryNumber { get; protected set; }
		public ZString CHIEFEntryDate { get; protected set; }
		public ZString CHIEFEntryTime { get; protected set; }
		public ZString RemovalType { get; protected set; }
		public ZString RemovalDestination { get; protected set; }
		public ZString RemovalCode { get; protected set; }

		#endregion

		public bool IsChiefEntryNumber => !CHIEFEntryEPU.IsEmpty;

		public ZString EntryNumber => IsChiefEntryNumber ? (ZString)$"{CHIEFEntryEPU}-{CHIEFEntryNumber}" : CHIEFEntryNumber.Trim();

		#endregion

		public ZDateTime ChopUpDateFromString(ZString dateAsASeriesOfIntegersPackedAsAString)
		{
			ZString stringIn = dateAsASeriesOfIntegersPackedAsAString;   // to make it a shorter var name once we are invisible

			if (!dateAsASeriesOfIntegersPackedAsAString.IsNumbersOnlyOrEmpty)
			{
				throw new FormatException("The format of the " + MessageType + " 'date' element was not entirely numeric. " +
																				"This is unacceptable. Input was [" + dateAsASeriesOfIntegersPackedAsAString + "]");
			}

			if (dateAsASeriesOfIntegersPackedAsAString.Length != 10)
			{
				throw new FormatException("The length of the " + MessageType + " 'date' element was not 10 characters. " +
																				 " This is unacceptable. Input was [" + dateAsASeriesOfIntegersPackedAsAString + "]");
			}

			return DateTime.ParseExact(dateAsASeriesOfIntegersPackedAsAString, "yyMMddHHmm", null);
		}

		protected virtual void ValidateMessage()
		{
			if (!ValidMessageIdentifiers.Select(x => ZString.Format("={0}", x)).Contains(MessageIdentifier))
			{
				throw new NotSupportedException(InvalidMessageException);
			}

			if (!messageReceivedFromCustoms.Contains(this.delimiter))
			{
				throw new FormatException(MissingDelimiterException);
			}

			if (messageReceivedFromCustoms.Split(this.delimiter).Length < MessageLength)
			{
				throw new FormatException(MessageLengthException);
			}
		}

		public virtual void SetUpInstanceOfMessageFromString()
		{
			ValidateMessage();

			MessageType = messageReceivedFromCustoms.Substring(1, 5);
			ZString[] elements = messageReceivedFromCustoms.Split(this.delimiter, this.delimiter);
			BerthCode = elements[1].TrimEnd();
			UCN = new UniqueConsignmentNumber(elements[2]); // this will take the full 14 chars and chop it up
			Container = elements[3].TrimEnd();
			ContainerNumber2 = elements[4].TrimEnd();
			MarksAndNumbers = elements[6].TrimEnd();
			Packages = ZInt.ParseEmptyAsZero(elements[7]);
			Weight = ZDecimal.Parse(elements[8]);
			DateOfStatusEvent = this.ChopUpDateFromString(elements[9]);
			BlNumber = elements[10].TrimEnd();
			UserName = elements[12].SubstringSafe(0, 20).TrimEnd();
			if (elements.Length > 13 && !elements[13].IsEmpty)
			{
				BlNumber = elements[13].SubstringSafe(0, 35).TrimEnd(); // full BL number
			}
			if (MessageType == "RRA11" && elements.Length > 21)
			{
				// January 2013 version of RRa11, for Destin8 release v3.5
				AgentsReference = elements[14];
				CHIEFEntryEPU = elements[15];
				CHIEFEntryNumber = elements[16];
				CHIEFEntryDate = elements[17];
				CHIEFEntryTime = elements[18];
				RemovalType = elements[19];
				RemovalDestination = elements[20];
				RemovalCode = elements[21].SubstringSafe(0, 3).TrimEnd();
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(MessageType + ":");
			sb.Append("UCN = ");
			sb.AppendLine(UCN.ProperlyTruncatedUCN);
			sb.Append("Raw UCN = ");
			sb.AppendLine(UCN.Raw);
			sb.Append("Cont = ");
			sb.AppendLine(Container);
			sb.Append("BoL = ");
			sb.AppendLine(BlNumber);
			sb.Append("M&N = ");
			sb.AppendLine(MarksAndNumbers);
			if (!CHIEFEntryNumber.IsEmpty)
			{
				sb.Append("CHIEF Entry = ");
				sb.Append(ChiefDetailsFormatted);
			}
			if (HasRemovalDetails)
			{
				sb.Append("Removal Details = ");
				sb.Append(RemovalDetailsFormatted);
			}
			return sb.ToString();
		}

		public bool HasRemovalDetails
		{
			get { return !RemovalCode.IsEmpty || !RemovalDestination.IsEmpty || !RemovalType.IsEmpty; }
		}

		public ZString RemovalDetailsFormatted
		{
			get
			{
				var removalCodeExplained = new RraRemovalCodes().GetDescriptionFromCode(RemovalCode.Left(1));
				return string.Format("Type={0}, Dest={1}, Code={2} ({3})", RemovalType, RemovalDestination, RemovalCode, removalCodeExplained);
			}
		}

		public ZString ChiefDetailsFormatted
		{
			get { return $"{EntryNumber} {CHIEFEntryDate} {CHIEFEntryTime}"; }
		}

		public virtual bool AreThereAnyHolds()
		{
			return false;
		}

		public virtual ZString HoldsString
		{
			get
			{
				return string.Empty;
			}
		}

		string IPortAuthorityHoldApplicationProvider.HoldType
		{
			get { return string.Empty; }
		}

		string IPortAuthorityHoldApplicationProvider.HoldAuthority
		{
			get { return string.Empty; }
		}

		AddOrRemove IPortAuthorityHoldApplicationProvider.DirectionOfApplication
		{
			get { return AddOrRemove.Cleared; }
		}

		ZDateTime IPortAuthorityHoldApplicationProvider.Date
		{
			get { return DateOfStatusEvent; }
		}

		public virtual void ApplyHoldsOrClear(CusEntryHeader entryHeader)
		{
			entryHeader.ApplyOrRemoveHoldOrClear(this);
		}

		public virtual void ProcessShipments(CusEntryHeader entryHeader)
		{
		}

		protected void AddClearedEventIfNeeded(CusEntryHeader entryHeader)
		{
			if (entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code)).Length == 0)
			{
				entryHeader.Logs.AddNew(Events.CustomsCleared, "Cleared via " + MessageType, DateOfStatusEvent.ToOffset());
			}
		}
	}
}
