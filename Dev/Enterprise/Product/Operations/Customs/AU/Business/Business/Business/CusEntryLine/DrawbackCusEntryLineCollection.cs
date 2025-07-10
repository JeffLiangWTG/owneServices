using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Collection of AU Customs Entry Lines used on a drawback line to calculate a representative value.
	/// These lines are loaded and saved in a note attached to the drawback invoice line.
	/// </summary>
	public class DrawbackCusEntryLineCollection : BusinessObjectCollection<CusEntryLine>
	{
		protected DrawbackCusEntryLineCollection(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
			: base(factory)
		{
			this.invoiceLine = invoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		public static DrawbackCusEntryLineCollection CreateDrawbackCusEntryLineCollection(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
		{
			DrawbackCusEntryLineCollection collection = new DrawbackCusEntryLineCollection(invoiceLine, factory);
			collection.LoadFromNote();
			collection.HasChanges = false;

			return collection;
		}
		protected override bool AllowNewCore
		{ get { return false; } }

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			CusEntryLine newLine = (CusEntryLine)bizOAdded;
			if (newLine.Declaration != null)
			{
				newLine.DrawbackClaimQuantity = newLine.CustomsUnitQty.IsEmpty ? newLine.InvoiceQuantity : newLine.Quantity;
			}

			if (!isLoadingCollection)
			{
				invoiceLine.HasChanges = true;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			invoiceLine.HasChanges = true;
		}

		#region Load and save
		public void LoadFromNote()
		{
			if (!DrawbackNote.IsEmpty)
			{
				using (invoiceLine.SuspendSettingHasChanges())
				{
					try
					{
						isLoadingCollection = true;
						ZString[] drawbackReferenceEntryLines = DrawbackNote.Split(new char[] { ',' });
						foreach (ZString drawbackReferenceEntryLine in drawbackReferenceEntryLines)
						{
							ZString[] elements = drawbackReferenceEntryLine.Split(new char[] { '*' });
							if (elements.Length == 3)
							{
								ZInt zintResult;
								ZInt.TryParse(elements[1], out zintResult);
								CusEntryLine entryLine = (CusEntryLine)invoiceLine.CusEntryLineLoader.FindByDeclarationAndLineNumbers(elements[0], zintResult);
								if (entryLine != null)
								{
									this.Add(entryLine);
									ZDecimal zDecimalResult;
									ZDecimal.TryParse(elements[2], out zDecimalResult);
									entryLine.DrawbackClaimQuantity = zDecimalResult;
									entryLine.HasChanges = false;
								}
							}
							else
							{
								throw new ArgumentException("DrawbackNoteLineShouldHaveThreeFields");
							}
						}
					}
					finally
					{
						isLoadingCollection = false;
					}
				}
			}
		}
		bool isLoadingCollection;

		public void SaveToNote()
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (CusEntryLine entryLine in this)
			{
				if (!result.IsEmpty)
				{
					result.Append(",");
				}

				result.Append(entryLine.Header.EntryNumber.Trim());
				result.Append("*");
				result.Append(entryLine.CL_LineNumber.ToString());
				result.Append("*");
				result.Append(entryLine.DrawbackClaimQuantity.ToString());
			}

			DrawbackNote = result.ToString();
		}

		[BusinessObjectTestExclude]
		public ZString DrawbackNote
		{
			get { return DrawbackNoteWriter.Value; }
			set { DrawbackNoteWriter.UpdateValue(value); }
		}

		PredefinedNoteWriter DrawbackNoteWriter
		{
			get { return drawbackNoteWriter ?? (drawbackNoteWriter = new PredefinedNoteWriter(invoiceLine, DrawbackEntryLinesNoteDescription, StmNoteVisibility.DOC)); }
		}
		PredefinedNoteWriter drawbackNoteWriter;

		internal const string DrawbackEntryLinesNoteDescription = "DrawbackEntryLines";

		#endregion
	}
}
