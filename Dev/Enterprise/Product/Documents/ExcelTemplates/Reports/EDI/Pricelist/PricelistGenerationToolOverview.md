---
title: Pricelist generation tool
css: ./css/pandoc.css
---

# Pricelist generation tool

## Contents
- [Overview](#overview)
- [Components](#components)
  * [Pricelist Master template](#pricelist-master-template)
  * [Pricelist Client template](#pricelist-client-template)
  * [Pricelist Controller](#pricelist-controller)

&nbsp;

# Overview
Client pricelists can be automatically generated using data from ediProd. This document lists the various components that make up the pricelist generation tool.

**NOTE:** The tool is not embedded as a system tool within ediProd. A base version is added to git for archiving purposes / future reference.
This is a customised report / tool that will be used and maintained by the Finance Team.

# Components
The tool is made up of 3 Excel templates.

## Pricelist Master Template
[PriceListMaster.xlsx](./PriceListMaster.xlsx) is a regular DocEngine report used to extract data out of ediProd and serves as the data source to generate the customers' pricelists.

## Pricelist Client Template
[PriceListClientTemplate.xlsx](./PriceListClientTemplate.xlsx) contains:
- static text information needed on the output
- formulas to translate/interpret the ediProd data
- price information tables for the pricelist output

## Pricelist Controller
[PriceListClientController.xlsm](./PriceListClientController.xlsm) processes information from the Pricelist Master and Pricelist Client templates to generate individual client pricelists in a PDF format.
It also generates the pricelist in a .xlsx format as an intermediary step.
