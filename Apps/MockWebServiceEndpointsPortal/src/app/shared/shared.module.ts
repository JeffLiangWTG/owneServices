import { NgModule } from '@angular/core';
import { FilterPipe } from './pipes/filter.pipe'
import { FormsModule } from '@angular/forms';

@NgModule({
	declarations: [
		FilterPipe
	],
	imports: [
		FormsModule
	],
	exports: [
		FilterPipe,
		FormsModule
	]
})

export class SharedModule { }